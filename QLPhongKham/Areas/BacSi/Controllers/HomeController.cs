using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Repositories;
using QLPhongKham.Services;
using QLPhongKham.Data;
namespace QLPhongKham.Areas.BacSi.Controllers
{
    [Area("BacSi")]
    [Authorize(Roles = "BacSi")]
    public class HomeController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IMedicalRecordRepository _medicalRecordRepo;
        private readonly IDoctorLinkingService _doctorLinkingService;
        private readonly IUserService _userService;

        public HomeController(
            IAppointmentRepository appointmentRepo,
            IMedicalRecordRepository medicalRecordRepo,
            IDoctorLinkingService doctorLinkingService,
            IUserService userService)
        {
            _appointmentRepo = appointmentRepo;
            _medicalRecordRepo = medicalRecordRepo;
            _doctorLinkingService = doctorLinkingService;
            _userService = userService;
        }        public async Task<IActionResult> Index()
        {
            var currentUser = await _userService.GetCurrentUserAsync();
            if (currentUser == null) return Forbid();
            
            var doctor = await _doctorLinkingService.GetDoctorByUserIdAsync(currentUser.Id);

            var todayAppointments = doctor != null
                ? _appointmentRepo.GetAppointmentsByDoctor(doctor.DoctorId)
                    .Where(a => a.AppointmentDate.Date == DateTime.Today).ToList()
                : new List<QLPhongKham.Models.Appointment>();

            ViewBag.TodayAppointmentsCount = todayAppointments.Count;
            ViewBag.PendingAppointments = doctor != null
                ? _appointmentRepo.GetAppointmentsByDoctor(doctor.DoctorId)
                    .Where(a => a.Status == "Scheduled").Count()
                : 0;

            return View(todayAppointments);
        }
    }
}
