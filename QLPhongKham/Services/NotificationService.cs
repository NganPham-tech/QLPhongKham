using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Services
{
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

        public async Task NotifyAppointmentCreatedAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment?.Patient?.Email == null) return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == appointment.Patient.Email);
            if (user == null) return;

            await CreateNotificationAsync(
                user.Id,
                "Lịch hẹn mới đã được tạo",
                $"Lịch hẹn của bạn với bác sĩ {appointment.Doctor.FullName} cho dịch vụ {appointment.Service.Name} vào lúc {appointment.AppointmentDate:dd/MM/yyyy HH:mm} đã được tạo thành công.",
                "Success",
                "/BenhNhan/Appointment",
                appointmentId
            );
        }

        public async Task NotifyAppointmentUpdatedAsync(int appointmentId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Doctor)
                .Include(a => a.Service)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment?.Patient?.Email == null) return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == appointment.Patient.Email);
            if (user == null) return;

            await CreateNotificationAsync(
                user.Id,
                "Lịch hẹn đã được cập nhật",
                $"Lịch hẹn của bạn với bác sĩ {appointment.Doctor.FullName} đã được cập nhật. Thời gian mới: {appointment.AppointmentDate:dd/MM/yyyy HH:mm}",
                "Info",
                "/BenhNhan/Appointment",
                appointmentId
            );
        }

        public async Task NotifyInvoiceCreatedAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice?.Patient?.Email == null) return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == invoice.Patient.Email);
            if (user == null) return;

            await CreateNotificationAsync(
                user.Id,
                "Hóa đơn mới",
                $"Hóa đơn {invoice.InvoiceNumber} với tổng tiền {invoice.TotalAmount:N0} VNĐ đã được tạo. Vui lòng thanh toán trong thời gian sớm nhất.",
                "Warning",
                "/BenhNhan/Invoice"
            );
        }

        public async Task NotifyPaymentReceivedAsync(int paymentId)
        {
            var payment = await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Patient)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment?.Invoice?.Patient?.Email == null) return;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == payment.Invoice.Patient.Email);
            if (user == null) return;

            await CreateNotificationAsync(
                user.Id,
                "Thanh toán đã được ghi nhận",
                $"Chúng tôi đã nhận được thanh toán {payment.AmountPaid:N0} VNĐ cho hóa đơn {payment.Invoice.InvoiceNumber}. Cảm ơn bạn!",
                "Success",
                "/BenhNhan/Payment"
            );
        }
    }
}
