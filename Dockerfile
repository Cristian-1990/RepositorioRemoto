# ============================================================
# Dockerfile multi-etapa para RepositorioRemoto
# ============================================================

# ETAPA 1: Build + Tests
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Copiar SOLO los ficheros de proyecto (NO obj/ ni bin/)
COPY RepositorioRemoto.slnx ./
COPY RepositorioRemoto/RepositorioRemoto.csproj ./RepositorioRemoto/
COPY RepositorioRemoto.Tests/RepositorioRemoto.Tests.csproj ./RepositorioRemoto.Tests/

# Restaurar paquetes (descarga limpia, sin cache de Windows)
RUN dotnet restore

# Copiar el codigo fuente
COPY RepositorioRemoto/ ./RepositorioRemoto/
COPY RepositorioRemoto.Tests/ ./RepositorioRemoto.Tests/

# Compilar
RUN dotnet build --configuration Release --no-restore

# EJECUTAR TESTS - si fallan, la imagen NO se crea.
# Se excluyen los de TestContainers (PostgreSQL y Redis): necesitan Docker dentro de Docker.
RUN dotnet test --configuration Release --no-restore --verbosity normal \
    --filter "FullyQualifiedName!~UserRepositoryTests&FullyQualifiedName!~RedisCacheServiceTests"

# Publicar
RUN dotnet publish RepositorioRemoto/RepositorioRemoto.csproj \
    --configuration Release \
    --no-restore \
    --output /app/publish

# ETAPA 2: Runtime (imagen ligera y sin root)
FROM mcr.microsoft.com/dotnet/runtime:10.0 AS runtime
WORKDIR /app

# Usuario no-root por seguridad, dueno de /app para poder escribir
RUN useradd -m appuser && chown appuser:appuser /app
USER appuser

# Copiar solo el binario publicado
COPY --from=build --chown=appuser:appuser /app/publish .

# Perfil por defecto. El docker-compose lo sobreescribe a Production.
ENV DOTNET_ENVIRONMENT=Development

ENTRYPOINT ["dotnet", "RepositorioRemoto.dll"]