using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Repositories;

namespace QLPhongKham.Areas.NhanVien
{
    [Area("NhanVien")]
    [Authorize(Roles = "NhanVien")]
    public class HomeController : Controller
    {
        private readonly IAppointmentRepository _appointmentRepo;
        private readonly IPatientRepository _patientRepo;

        public HomeController(
            IAppointmentRepository appointmentRepo,
            IPatientRepository patientRepo)
        {
            _appointmentRepo = appointmentRepo;
            _patientRepo = patientRepo;
        }

        public IActionResult Index()
        {
            var todayAppointments = _appointmentRepo.GetTodayAppointments().ToList();
            var totalPatients = _patientRepo.GetAll().Count();

            ViewBag.TodayAppointmentsCount = todayAppointments.Count;
            ViewBag.TotalPatients = totalPatients;

            return View(todayAppointments);
        }
    }
}
