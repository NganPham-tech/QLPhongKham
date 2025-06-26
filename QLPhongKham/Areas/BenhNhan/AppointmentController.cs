using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Models;
using QLPhongKham.Repositories;
using QLPhongKham.Services;

namespace QLPhongKham.Areas.BenhNhan
{
    [Area("BenhNhan")]
    [Authorize(Roles = "BenhNhan")]
    public class AppointmentController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IDoctorRepository _doctorRepo;
        private readonly IServiceRepository _serviceRepo;
        private readonly IPatientLinkingService _patientLinkingService;
        private readonly IUserService _userService;

        public AppointmentController(
            IAppointmentRepository appointmentRepo,
            IDoctorRepository doctorRepo,
            IServiceRepository serviceRepo,
            IPatientLinkingService patientLinkingService,
            IUserService userService)
        {
            _appointmentRepo = appointmentRepo;
            _doctorRepo = doctorRepo;
            _serviceRepo = serviceRepo;
            _patientLinkingService = patientLinkingService;
            _userService = userService;
        }        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null) return Forbid();
            
            var patient = await _patientLinkingService.GetPatientByUserIdAsync(currentUser.Id);

            var appointments = patient != null
                ? _appointmentRepo.GetAppointmentsByPatient(patient.PatientId).ToList()
                : new List<Appointment>();

            return View(appointments);
        }

        [HttpGet]
        public IActionResult Create()
        {
            ViewBag.Doctors = _doctorRepo.GetAll().ToList();
            ViewBag.Services = _serviceRepo.GetAll().ToList();

            var model = new Appointment
            {
                AppointmentDate = DateTime.Now.AddDays(1)
            };

            return View(model);
        }

        [HttpPost]        public async Task<IActionResult> Create(Appointment model)
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null) return Forbid();
            
            var patient = await _patientLinkingService.GetPatientByUserIdAsync(currentUser.Id);

            if (patient == null)
            {
                // Tạo patient record nếu chưa có
                patient = await _patientLinkingService.CreatePatientForUserAsync(currentUser);
            }

            if (ModelState.IsValid)
            {
                model.PatientId = patient.PatientId;
                model.Status = "Scheduled";

                _appointmentRepo.Add(model);
                TempData["Success"] = "Đặt lịch khám thành công!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.Doctors = _doctorRepo.GetAll().ToList();
            ViewBag.Services = _serviceRepo.GetAll().ToList();
            return View(model);
        }
    }
}
