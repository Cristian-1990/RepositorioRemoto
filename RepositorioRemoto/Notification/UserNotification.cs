using RepositorioRemoto.Models;

namespace RepositorioRemoto.Notification;

public record UserNotification(
    NotificationType Type,
    User User,
    DateTime Date
    );