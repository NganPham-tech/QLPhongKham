using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Controllers
{
    public class PatientPortalController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PatientPortalController(ApplicationDbContext context)
        {
            _context = context;
        }        // Trang chủ cho bệnh nhân
        public async Task<IActionResult> Index(bool? bookingSuccess = null)
        {
            var featuredServices = await _context.Services
                .Where(s => s.IsActive)
                .Take(6)
                .ToListAsync();

            var featuredDoctors = await _context.Doctors
                .Where(d => d.IsActive)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedServices = featuredServices;
            ViewBag.FeaturedDoctors = featuredDoctors;
            
            // Display success message if redirected from successful booking
            if (bookingSuccess == true)
            {
                TempData["Success"] = "Đặt lịch hẹn thành công! Chúng tôi sẽ liên hệ xác nhận trong thời gian sớm nhất.";
            }

            return View();
        }

        // Trang danh sách bác sĩ
        public async Task<IActionResult> Doctors(string specialty = "")
        {
            var doctors = _context.Doctors.Where(d => d.IsActive);

            if (!string.IsNullOrEmpty(specialty))
            {
                doctors = doctors.Where(d => d.Specialty == specialty);
            }

            var doctorList = await doctors
                .OrderBy(d => d.FirstName)
                .ToListAsync();

            
            var specialties = await _context.Doctors
                .Where(d => d.IsActive && !string.IsNullOrEmpty(d.Specialty))
                .Select(d => d.Specialty)
                .Distinct()
                .ToListAsync();

            ViewBag.Specialties = specialties;
            ViewBag.CurrentSpecialty = specialty;

            return View(doctorList);
        }

        // Chi tiết bác sĩ
        public async Task<IActionResult> DoctorDetail(int id)
        {
            var doctor = await _context.Doctors
                .Where(d => d.IsActive && d.DoctorId == id)
                .FirstOrDefaultAsync();

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

        // Chi tiết dịch vụ
        public async Task<IActionResult> ServiceDetail(int id)
        {
            var service = await _context.Services
                .Where(s => s.IsActive && s.ServiceId == id)
                .FirstOrDefaultAsync();

            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }        // Trang đặt lịch hẹn (yêu cầu đăng nhập)
        public async Task<IActionResult> BookAppointment()
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return RedirectToAction("Login", "Account", new { area = "Identity", returnUrl = Url.Action("BookAppointment") });
            }

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
        [HttpGet]
        public async Task<IActionResult> GetDoctorsByService(int serviceId)
        {
            try
            {
                var service = await _context.Services
                    .FirstOrDefaultAsync(s => s.ServiceId == serviceId && s.IsActive);

                if (service == null)
                {
                    return Json(new { success = false, message = "Dịch vụ không tồn tại" });
                }

                var query = _context.Doctors.Where(d => d.IsActive);

                // Nếu dịch vụ có chuyên khoa cụ thể, lọc bác sĩ theo chuyên khoa
                if (!string.IsNullOrEmpty(service.Specialty))
                {
                    query = query.Where(d => d.Specialty == service.Specialty);
                }

                var doctors = await query
                    .OrderBy(d => d.FirstName)
                    .Select(d => new {
                        doctorId = d.DoctorId,
                        fullName = d.FullName,
                        specialty = d.Specialty,
                        photoUrl = d.PhotoUrl
                    })
                    .ToListAsync();

                return Json(new { 
                    success = true, 
                    doctors = doctors,
                    serviceSpecialty = service.Specialty 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Lỗi: {ex.Message}" });
            }
        }

        // Xử lý đặt lịch hẹn - nhận ngày và giờ riêng biệt để tránh vấn đề timezone
        [HttpPost]
        public async Task<IActionResult> BookAppointment(int doctorId, int serviceId, string appointmentDate, string appointmentTime, string notes)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đặt lịch hẹn" });
            }

            try
            {
                // Parse ngày và giờ riêng biệt để tránh vấn đề timezone
                if (!DateTime.TryParse(appointmentDate, out var dateOnly))
                {
                    return Json(new { success = false, message = "Ngày không hợp lệ" });
                }
                
                if (!TimeSpan.TryParse(appointmentTime, out var timeOnly))
                {
                    return Json(new { success = false, message = "Giờ không hợp lệ" });
                }
                
                // Tạo DateTime từ ngày và giờ local
                var appointmentDateTime = dateOnly.Date.Add(timeOnly);
                
                // Debug log để kiểm tra DateTime sau khi parse
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Original date string: {appointmentDate}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Original time string: {appointmentTime}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Parsed DateTime: {appointmentDateTime}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] DateTime Kind: {appointmentDateTime.Kind}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Date part: {appointmentDateTime.Date}");
                System.Diagnostics.Debug.WriteLine($"[DEBUG] Time part: {appointmentDateTime.TimeOfDay}");
                
                // Kiểm tra giờ làm việc trước khi xử lý
                if (!IsValidWorkingTime(appointmentDateTime))
                {
                    var errorMsg = GetWorkingTimeErrorMessage(appointmentDateTime);
                    System.Diagnostics.Debug.WriteLine($"[DEBUG] Working time validation failed: {errorMsg}");
                    return Json(new { success = false, message = errorMsg });
                }

                // Tìm patient theo user email
                var userEmail = User.Identity.Name;
                var patient = await _context.Patients
                    .FirstOrDefaultAsync(p => p.Email == userEmail);

                if (patient == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin bệnh nhân. Vui lòng cập nhật hồ sơ." });
                }

                // Kiểm tra xung đột lịch hẹn
                var appointmentStart = appointmentDateTime.AddMinutes(-30);
                var appointmentEnd = appointmentDateTime.AddMinutes(30);
                
                var conflictingAppointment = await _context.Appointments
                    .Where(a => a.DoctorId == doctorId && 
                               a.AppointmentDate >= appointmentStart &&
                               a.AppointmentDate <= appointmentEnd &&
                               a.Status != "Cancelled")
                    .FirstOrDefaultAsync();

                if (conflictingAppointment != null)
                {
                    return Json(new { success = false, message = "Bác sĩ đã có lịch hẹn trong khoảng thời gian này. Vui lòng chọn giờ khác." });
                }

                // Tạo lịch hẹn mới
                var appointment = new Appointment
                {
                    PatientId = patient.PatientId,
                    DoctorId = doctorId,
                    ServiceId = serviceId,
                    AppointmentDate = appointmentDateTime,
                    Status = "Pending",
                    Notes = notes ?? "",
                    CreatedDate = DateTime.Now
                };

                _context.Add(appointment);
                await _context.SaveChangesAsync();

                return Json(new { success = true, message = "Đặt lịch hẹn thành công! Chúng tôi sẽ liên hệ xác nhận trong thời gian sớm nhất." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = $"Có lỗi xảy ra: {ex.Message}" });
            }
        }

        // Kiểm tra thời gian đặt lịch có nằm trong giờ làm việc không
        private bool IsValidWorkingTime(DateTime appointmentDate)
        {
            // Kiểm tra ngày trong tương lai
            if (appointmentDate.Date <= DateTime.Now.Date)
            {
                return false;
            }

            // Kiểm tra không phải cuối tuần (Thứ 7, Chủ nhật)
            if (appointmentDate.DayOfWeek == DayOfWeek.Saturday || appointmentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return false;
            }

            // Kiểm tra giờ làm việc (8:00 - 17:00, bao gồm 17:00)
            var timeOfDay = appointmentDate.TimeOfDay;
            var startTime = new TimeSpan(8, 0, 0);  // 8:00 AM
            var endTime = new TimeSpan(17, 0, 0);   // 5:00 PM

            if (timeOfDay < startTime || timeOfDay > endTime)
            {
                return false;
            }

            // Kiểm tra các ngày lễ cố định
            if (IsHoliday(appointmentDate))
            {
                return false;
            }

            return true;
        }

        // Trả về thông báo lỗi tương ứng với lý do không hợp lệ
        private string GetWorkingTimeErrorMessage(DateTime appointmentDate)
        {
            if (appointmentDate.Date <= DateTime.Now.Date)
            {
                return "Không thể đặt lịch hẹn cho ngày hôm nay hoặc quá khứ. Vui lòng chọn ngày trong tương lai.";
            }

            if (appointmentDate.DayOfWeek == DayOfWeek.Saturday || appointmentDate.DayOfWeek == DayOfWeek.Sunday)
            {
                return "Phòng khám không làm việc vào cuối tuần. Vui lòng chọn ngày từ Thứ 2 đến Thứ 6.";
            }

            var timeOfDay = appointmentDate.TimeOfDay;
            var startTime = new TimeSpan(8, 0, 0);
            var endTime = new TimeSpan(17, 0, 0);

            if (timeOfDay < startTime || timeOfDay > endTime)
            {
                return "Phòng khám chỉ làm việc từ 8:00 đến 17:00 (bao gồm cả 17:00). Vui lòng chọn giờ trong khoảng thời gian này.";
            }

            if (IsHoliday(appointmentDate))
            {
                return "Ngày bạn chọn là ngày lễ. Phòng khám không làm việc vào ngày này.";
            }

            return "Thời gian đặt lịch không hợp lệ.";
        }

        // Kiểm tra ngày lễ
        private bool IsHoliday(DateTime date)
        {
            // Các ngày lễ cố định trong năm
            var holidays = new List<(int Month, int Day)>
            {
                (1, 1),   // Tết Dương lịch
                (4, 30),  // Giải phóng miền Nam
                (5, 1),   // Quốc tế Lao động
                (9, 2),   // Quốc khánh
                (12, 25)  // Noel
            };

            return holidays.Any(h => date.Month == h.Month && date.Day == h.Day);
        }
    }
}
