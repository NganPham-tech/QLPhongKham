using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
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
    }

    public class NotificationService : INotificationService
    {
        private readonly ApplicationDbContext _context;

        public NotificationService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task CreateNotificationAsync(string userId, string title, string message, string type = "Info", string? relatedUrl = null, int? appointmentId = null)
        {
            var notification = new Notification
            {
                UserId = userId,
                Title = title,
                Message = message,
                Type = type,
                RelatedUrl = relatedUrl,
                AppointmentId = appointmentId,
                CreatedAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
        }        public async Task<List<Notification>> GetUserNotificationsAsync(string userId, bool unreadOnly = false)
        {
            var query = _context.Notifications
                .Include(n => n.Appointment)
                    .ThenInclude(a => a!.Doctor)
                .Include(n => n.Appointment)
                    .ThenInclude(a => a!.Service)
                .Where(n => n.UserId == userId);

            if (unreadOnly)
            {
                query = query.Where(n => !n.IsRead);
            }

            return await query
                .OrderByDescending(n => n.CreatedAt)
                .ToListAsync();
        }

        public async Task MarkAsReadAsync(int notificationId)
        {
            var notification = await _context.Notifications.FindAsync(notificationId);
            if (notification != null)
            {
                notification.IsRead = true;
                await _context.SaveChangesAsync();
            }
        }

        public async Task<int> GetUnreadCountAsync(string userId)
        {
            return await _context.Notifications
                .CountAsync(n => n.UserId == userId && !n.IsRead);
        }

        public async Task NotifyAppointmentStatusChangeAsync(int appointmentId, string newStatus)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment?.Patient?.Email == null) return;

            // Tìm user ID từ email của patient
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == appointment.Patient.Email);
            if (user == null) return;

            string title = "";
            string message = "";
            string type = "Info";

            switch (newStatus.ToLower())
            {
                case "completed":
                    title = "Lịch hẹn đã hoàn thành";
                    message = $"Lịch hẹn của bạn với bác sĩ {appointment.Doctor.FullName} đã được hoàn thành thành công.";
                    type = "Success";
                    break;
                case "cancelled":
                    title = "Lịch hẹn đã bị hủy";
                    message = $"Lịch hẹn của bạn với bác sĩ {appointment.Doctor.FullName} đã bị hủy. Vui lòng liên hệ để biết thêm chi tiết.";
                    type = "Warning";
                    break;
                case "in progress":
                    title = "Lịch hẹn đang được thực hiện";
                    message = $"Bác sĩ {appointment.Doctor.FullName} đang thực hiện khám cho bạn.";
                    type = "Info";
                    break;
                case "pending":
                    title = "Lịch hẹn đã được xác nhận";
                    message = $"Lịch hẹn của bạn với bác sĩ {appointment.Doctor.FullName} vào lúc {appointment.AppointmentDate:dd/MM/yyyy HH:mm} đã được xác nhận.";
                    type = "Success";
                    break;
                default:
                    title = "Cập nhật lịch hẹn";
                    message = $"Trạng thái lịch hẹn của bạn với bác sĩ {appointment.Doctor.FullName} đã được cập nhật thành: {newStatus}";
                    type = "Info";
                    break;
            }

            await CreateNotificationAsync(
                user.Id,
                title,
                message,
                type,
                "/BenhNhan/Appointment", // URL để xem lịch hẹn
                appointmentId
            );
        }
    }
}
