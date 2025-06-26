using QLPhongKham.Models;

namespace QLPhongKham.Services
{
    public interface INotificationService
    {
        Task CreateNotificationAsync(string userId, string title, string message, string type = "Info", string? relatedUrl = null, int? appointmentId = null);
        Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false);
        Task MarkAsReadAsync(int notificationId);
        Task<int> GetUnreadCountAsync(string userId);
        Task NotifyAppointmentStatusChangeAsync(int appointmentId, string newStatus);
        Task NotifyAppointmentCreatedAsync(int appointmentId);
        Task NotifyAppointmentUpdatedAsync(int appointmentId);
        Task NotifyInvoiceCreatedAsync(int invoiceId);
        Task NotifyPaymentReceivedAsync(int paymentId);
    }
}
