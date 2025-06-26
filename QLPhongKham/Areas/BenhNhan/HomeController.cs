using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Repositories;
using QLPhongKham.Services;

namespace QLPhongKham.Areas.BenhNhan
{
    [Area("BenhNhan")]
    [Authorize(Roles = "BenhNhan")]
    public class HomeController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IMedicalRecordRepository _medicalRecordRepo;
        private readonly IPatientLinkingService _patientLinkingService;
        private readonly IUserService _userService;

        public HomeController(
            IAppointmentRepository appointmentRepo,
            IMedicalRecordRepository medicalRecordRepo,
            IPatientLinkingService patientLinkingService,
            IUserService userService)
        {
            _appointmentRepo = appointmentRepo;
            _medicalRecordRepo = medicalRecordRepo;
            _patientLinkingService = patientLinkingService;
            _userService = userService;
        }        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null) return Forbid();
            
            var patient = await _patientLinkingService.GetPatientByUserIdAsync(currentUser.Id);

            var upcomingAppointments = patient != null
                ? _appointmentRepo.GetAppointmentsByPatient(patient.PatientId)
                    .Where(a => a.AppointmentDate > DateTime.Now && a.Status != "Cancelled")
                    .OrderBy(a => a.AppointmentDate)
                    .Take(5).ToList()
                : new List<QLPhongKham.Models.Appointment>();

            ViewBag.UpcomingCount = upcomingAppointments.Count;

            return View(upcomingAppointments);
        }
    }
}
