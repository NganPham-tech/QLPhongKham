using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.Repositories;
using QLPhongKham.Services;

namespace QLPhongKham.Areas.NhanVien
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVien")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IPatientRepository _patientRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IInvoiceService _invoiceService;
        private readonly IUserService _userService;
        private readonly ApplicationDbContext _context;

        public AppointmentController(
            IAppointmentRepository appointmentRepo,
            IPatientRepository patientRepo,
            IDoctorRepository doctorRepo,
            IServiceRepository serviceRepo,
            IInvoiceService invoiceService,
            IUserService userService,
            ApplicationDbContext context)
        {
            _appointmentRepo = appointmentRepo;
            _patientRepo = patientRepo;
            _doctorRepo = doctorRepo;
            _serviceRepo = serviceRepo;
            _invoiceService = invoiceService;
            _userService = userService;
            _context = context;
        }

        public IActionResult Index()
        {
            var appointments = _appointmentRepo.GetAll().ToList();
            return View(appointments);
        }

        public IActionResult Details(int id)
        {
            var appointment = _appointmentRepo.GetById(id);
            if (appointment == null) return NotFound();

            return View(appointment);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Patients = _patientRepo.GetAll().ToList();
            ViewBag.Doctors = _doctorRepo.GetAll().ToList();
            ViewBag.Services = _serviceRepo.GetAll().ToList();

            return View(new Appointment { AppointmentDate = DateTime.Now.AddDays(1) });
        }

        [HttpPost]
        public IActionResult Create(Appointment model)
        {
            if (ModelState.IsValid)
            {
                model.Status = "Scheduled";
                _appointmentRepo.Add(model);
                TempData["Success"] = "Tạo lịch hẹn thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Patients = _patientRepo.GetAll().ToList();
            ViewBag.Doctors = _doctorRepo.GetAll().ToList();
            ViewBag.Services = _serviceRepo.GetAll().ToList();
            return View(model);
        }

        [HttpGet]
        public IActionResult Edit(int id)
        {
            var appointment = _appointmentRepo.GetById(id);
            if (appointment == null) return NotFound();

            ViewBag.Patients = _patientRepo.GetAll().ToList();
            ViewBag.Doctors = _doctorRepo.GetAll().ToList();
            ViewBag.Services = _serviceRepo.GetAll().ToList();

            return View(appointment);
        }        [HttpPost]
        public async Task<IActionResult> Edit(Appointment model)
        {
            if (ModelState.IsValid)
            {
                // Lấy appointment cũ để so sánh status
                var oldAppointment = _appointmentRepo.GetById(model.AppointmentId);
                var oldStatus = oldAppointment?.Status;

                _appointmentRepo.Update(model);

                // Tự động tạo hóa đơn khi appointment hoàn thành
                if (model.Status == "Completed" && oldStatus != "Completed")
                {
                    try
                    {
                        // Kiểm tra xem đã có hóa đơn cho appointment này chưa
                        var existingInvoice = await _context.Invoices
                            .FirstOrDefaultAsync(i => i.AppointmentId == model.AppointmentId);

                        if (existingInvoice == null)
                        {
                            var staffId = await GetCurrentStaffIdAsync();
                            if (staffId.HasValue)
                            {
                                var invoice = await _invoiceService.CreateInvoiceFromAppointmentAsync(
                                    model.AppointmentId, 
                                    staffId.Value);
                                
                                TempData["Info"] = $"Hóa đơn {invoice.InvoiceNumber} đã được tạo tự động.";
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        TempData["Warning"] = "Cập nhật lịch hẹn thành công nhưng không thể tạo hóa đơn tự động.";
                        Console.WriteLine($"Failed to create invoice for appointment {model.AppointmentId}: {ex.Message}");
                    }
                }

                TempData["Success"] = "Cập nhật lịch hẹn thành công!";
                return RedirectToAction(nameof(Details), new { id = model.AppointmentId });
            }

            ViewBag.Patients = _patientRepo.GetAll().ToList();
            ViewBag.Doctors = _doctorRepo.GetAll().ToList();
            ViewBag.Services = _serviceRepo.GetAll().ToList();
            return View(model);
        }

        // Helper method to get current staff ID
        private async Task<int?> GetCurrentStaffIdAsync()
        {
            try
            {
                var currentUser = await _userService.GetCurrentUserAsync();
                if (currentUser == null) return null;

                // Tìm staff record theo email của user hiện tại
                var staff = await _context.Staffs
                    .FirstOrDefaultAsync(s => s.Email == currentUser.Email);

                return staff?.StaffId ?? 1; // Fallback to staff ID 1 if not found
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error getting current staff ID: {ex.Message}");
                return 1; // Fallback to staff ID 1
            }
        }
    }
}
