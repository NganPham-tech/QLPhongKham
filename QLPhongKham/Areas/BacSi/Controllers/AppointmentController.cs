using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.Repositories;
using QLPhongKham.Services;

namespace QLPhongKham.Areas.BacSi.Controllers
{
    [Area("BacSi")]
    [Authorize(Roles = "BacSi")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IMedicalRecordRepository _medicalRecordRepo;
        private readonly IDoctorLinkingService _doctorLinkingService;
        private readonly IUserService _userService;
        private readonly IInvoiceService _invoiceService;        private readonly ApplicationDbContext _context;

        public AppointmentController(
            IAppointmentRepository appointmentRepo,
            IMedicalRecordRepository medicalRecordRepo,
            IDoctorLinkingService doctorLinkingService,
            IUserService userService,
            IInvoiceService invoiceService,
            ApplicationDbContext context)
        {
            _appointmentRepo = appointmentRepo;
            _medicalRecordRepo = medicalRecordRepo;
            _doctorLinkingService = doctorLinkingService;
            _userService = userService;
            _invoiceService = invoiceService;
            _context = context;
        }        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null) return Forbid();
            
            var doctor = await _doctorLinkingService.GetDoctorByUserIdAsync(currentUser.Id);

            var appointments = doctor != null
                ? _appointmentRepo.GetAppointmentsByDoctor(doctor.DoctorId).ToList()
                : new List<Appointment>();

            return View(appointments);
        }

        public async Task<IActionResult> Details(int id)
        {
            var appointment = _appointmentRepo.GetById(id);
            if (appointment == null) return NotFound();            
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null) return Forbid();
            
            var doctor = await _doctorLinkingService.GetDoctorByUserIdAsync(currentUser.Id);

            if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                return Forbid();

            return View(appointment);
        }        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id, string status)
        {
            var appointment = _appointmentRepo.GetById(id);
            if (appointment == null) return NotFound();            
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null) return Forbid();
            
            var doctor = await _doctorLinkingService.GetDoctorByUserIdAsync(currentUser.Id);

            if (doctor == null || appointment.DoctorId != doctor.DoctorId)
                return Forbid();

            var oldStatus = appointment.Status;
            appointment.Status = status;
            _appointmentRepo.Update(appointment);

            // Tự động tạo hóa đơn khi appointment hoàn thành
            if (status == "Completed" && oldStatus != "Completed")
            {
                try
                {
                    // Kiểm tra xem đã có hóa đơn cho appointment này chưa
                    var existingInvoice = await _context.Invoices
                        .FirstOrDefaultAsync(i => i.AppointmentId == appointment.AppointmentId);

                    if (existingInvoice == null)
                    {
                        // Sử dụng DoctorId hiện tại làm staffId (tạm thời)
                        // Hoặc tìm staff record tương ứng với doctor
                        var staffId = await GetStaffIdFromDoctorAsync(doctor.DoctorId);
                        if (staffId.HasValue)
                        {
                            var invoice = await _invoiceService.CreateInvoiceFromAppointmentAsync(
                                appointment.AppointmentId, 
                                staffId.Value);
                            
                            TempData["Info"] = $"Hóa đơn {invoice.InvoiceNumber} đã được tạo tự động.";
                        }
                    }
                }
                catch (Exception ex)
                {
                    TempData["Warning"] = "Cập nhật trạng thái thành công nhưng không thể tạo hóa đơn tự động.";
                    Console.WriteLine($"Failed to create invoice for appointment {appointment.AppointmentId}: {ex.Message}");
                }
            }

            TempData["Success"] = "Cập nhật trạng thái thành công!";
            return RedirectToAction(nameof(Details), new { id });
        }

        // Helper method to get staff ID from doctor ID
        private async Task<int?> GetStaffIdFromDoctorAsync(int doctorId)
        {
            try
            {
                // Tìm doctor record để lấy email
                var doctor = await _context.Doctors.FindAsync(doctorId);
                if (doctor?.Email == null) return null;

                // Tìm staff record có cùng email (nếu doctor cũng là staff)
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.Email == doctor.Email);

                return staff?.StaffId ?? 1; // Fallback to staff ID 1 if not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting staff ID from doctor {doctorId}: {ex.Message}");
                return 1; // Fallback to staff ID 1
            }
        }
    }
}
