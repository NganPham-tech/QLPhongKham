using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Controllers
{
    public class PatientController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Trang chủ dành cho bệnh nhân
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .Take(6)
                .ToListAsync();

            ViewBag.Services = services;
            return View();
        }

        // Trang danh sách bác sĩ
        public async Task<IActionResult> Doctors()
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FirstName)
                .ToListAsync();

            return View(doctors);
        }

        // Trang chi tiết bác sĩ
        public async Task<IActionResult> DoctorDetail(int id)
        {
            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(d => d.DoctorId == id && d.IsActive);

            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // Trang danh sách dịch vụ
        public async Task<IActionResult> Services()
        {
            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            return View(services);
        }

        // Trang chi tiết dịch vụ
        public async Task<IActionResult> ServiceDetail(int id)
        {
            var service = await _context.Services
                .FirstOrDefaultAsync(s => s.ServiceId == id && s.IsActive);

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // Trang đặt lịch khám
        public async Task<IActionResult> BookAppointment()
        {
            var doctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FirstName)
                .ToListAsync();

            var services = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .ToListAsync();

            ViewBag.Doctors = doctors;
            ViewBag.Services = services;

            return View();
        }
    }
}
