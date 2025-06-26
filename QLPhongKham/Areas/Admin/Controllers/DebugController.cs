using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class DebugController : Controller
    {
        private readonly ApplicationDbContext _context;

        public DebugController(ApplicationDbContext context)
        {
            _context = context;
        }

       
        public async Task<IActionResult> FixDoctorStatus()
        {
            try
            {
                var doctors = await _context.Doctors.ToListAsync();
                foreach (var doctor in doctors)
                {
                    doctor.IsActive = true;
                }
                
                await _context.SaveChangesAsync();
                
                var activeCount = await _context.Doctors.CountAsync(d => d.IsActive);
                
                return Content($"Success: Đã cập nhật {doctors.Count} bác sĩ thành IsActive = true. Hiện có {activeCount} bác sĩ active.");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}");
            }
        }

        // Action đơn giản để cập nhật doctor status
        [HttpGet]
        public async Task<IActionResult> UpdateDoctorStatus()
        {
            var result = "";
            try
            {
                // Lấy tất cả bác sĩ
                var doctors = await _context.Doctors.ToListAsync();
                result += $"Found {doctors.Count} doctors.\n";

                // Cập nhật từng bác sĩ
                foreach (var doctor in doctors)
                {
                    result += $"Doctor {doctor.FirstName} {doctor.LastName}: IsActive was {doctor.IsActive}";
                    doctor.IsActive = true;
                    result += $", now {doctor.IsActive}\n";
                }

                // Lưu thay đổi
                var saved = await _context.SaveChangesAsync();
                result += $"Saved {saved} changes.\n";

                // Kiểm tra lại
                var activeCount = await _context.Doctors.CountAsync(d => d.IsActive);
                result += $"Active doctors now: {activeCount}\n";

                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                result += $"Error: {ex.Message}\n{ex.StackTrace}";
                return Content(result, "text/plain");
            }
        }

        // Kiểm tra số lượng bác sĩ
        public async Task<IActionResult> CheckDoctors()
        {
            var totalDoctors = await _context.Doctors.CountAsync();
            var activeDoctors = await _context.Doctors.CountAsync(d => d.IsActive);
            var inactiveDoctors = await _context.Doctors.CountAsync(d => !d.IsActive);
            
            var doctorList = await _context.Doctors.Select(d => new { 
                d.DoctorId, 
                d.FirstName, 
                d.LastName, 
                d.IsActive 
            }).ToListAsync();

            var result = $@"
Tổng số bác sĩ: {totalDoctors}
Bác sĩ active: {activeDoctors}
Bác sĩ inactive: {inactiveDoctors}

Danh sách bác sĩ:
{string.Join("\n", doctorList.Select(d => $"ID: {d.DoctorId}, Tên: {d.FirstName} {d.LastName}, Active: {d.IsActive}"))}
            ";            return Content(result);
        }

        // Cập nhật bằng SQL trực tiếp
        [HttpGet]
        public async Task<IActionResult> UpdateWithSQL()
        {
            try
            {
                // Kiểm tra trước khi cập nhật
                var beforeCount = await _context.Doctors.CountAsync(d => d.IsActive);
                var totalCount = await _context.Doctors.CountAsync();
                
                // Cập nhật bằng SQL
                var rowsAffected = await _context.Database.ExecuteSqlRawAsync(
                    "UPDATE Doctors SET IsActive = 1");
                
                // Kiểm tra sau khi cập nhật
                var afterCount = await _context.Doctors.CountAsync(d => d.IsActive);
                
                var result = $@"
Before update:
- Total doctors: {totalCount}
- Active doctors: {beforeCount}

SQL Update result:
- Rows affected: {rowsAffected}

After update:
- Active doctors: {afterCount}
                ";
                
                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}\n{ex.StackTrace}", "text/plain");
            }
        }

        // Kiểm tra kết nối database
        [HttpGet]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Test connection
                var canConnect = await _context.Database.CanConnectAsync();
                var connectionString = _context.Database.GetConnectionString();
                
                // Get database info
                var doctorTableExists = await _context.Database.ExecuteSqlRawAsync(
                    "SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Doctors'") >= 0;
                
                // Get doctor data directly
                var doctorData = await _context.Database.SqlQueryRaw<DoctorInfo>(
                    "SELECT DoctorId, FirstName, LastName, IsActive FROM Doctors").ToListAsync();
                
                var result = $@"
Database Connection Test:
- Can connect: {canConnect}
- Connection string: {connectionString}
- Doctor table exists: {doctorTableExists}

Doctor data from SQL:
{string.Join("\n", doctorData.Select(d => $"ID: {d.DoctorId}, Name: {d.FirstName} {d.LastName}, Active: {d.IsActive}"))}
                ";
                
                return Content(result, "text/plain");
            }
            catch (Exception ex)
            {
                return Content($"Error: {ex.Message}\n{ex.StackTrace}", "text/plain");
            }
        }

        [HttpGet]
        [Route("/Admin/Debug/CheckRoutes")]        public IActionResult CheckRoutes()
        {
            // Kiểm tra các routes hiện tại
            var routes = new Dictionary<string, string>
            {
                { "Admin Home", Url.Action("Index", "Home", new { area = "Admin" }) ?? "/Admin/Home/Index" },
                { "Admin Root", "/Admin" },
                { "Default Home", Url.Action("Index", "Home", new { area = "" }) ?? "/Home/Index" },
                { "Patient Home", Url.Action("Index", "PatientHome", new { area = "" }) ?? "/PatientHome/Index" },
                { "Login", Url.Page("/Account/Login", new { area = "Identity" }) ?? "/Identity/Account/Login" },
                { "Current Path", HttpContext.Request.Path },
                { "Current URL", $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}{HttpContext.Request.QueryString}" }
            };

            return Json(routes);
        }

        [HttpGet]
        [Route("/Admin/Debug/BreakRedirectLoop")]
        public IActionResult BreakRedirectLoop()
        {
            // Trang này phá vỡ vòng lặp chuyển hướng nếu có
            var info = new {
                IsAuthenticated = User.Identity?.IsAuthenticated,
                UserName = User.Identity?.Name,
                IsAdmin = User.IsInRole("Admin"),
                CurrentUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{HttpContext.Request.Path}"
            };
            
            return Content($@"
                <html>
                <head><title>Redirect Loop Debugger</title></head>
                <body>
                    <h1>Đã phá vỡ vòng lặp chuyển hướng!</h1>
                    <h2>Thông tin người dùng:</h2>
                    <ul>
                        <li>Đã đăng nhập: {info.IsAuthenticated}</li>
                        <li>Tên người dùng: {info.UserName}</li>
                        <li>Là Admin: {info.IsAdmin}</li>
                        <li>URL hiện tại: {info.CurrentUrl}</li>
                    </ul>
                    <h2>Điều hướng:</h2>
                    <ul>
                        <li><a href='/'>Trang chủ</a></li>
                        <li><a href='/Admin/Home/Index'>Admin Dashboard</a></li>
                        <li><a href='/Identity/Account/Login'>Đăng nhập</a></li>
                        <li><a href='/Identity/Account/Logout'>Đăng xuất</a></li>
                    </ul>
                </body>
                </html>
            ", "text/html");
        }
          public class DoctorInfo
        {
            public int DoctorId { get; set; }
            public string FirstName { get; set; } = "";
            public string LastName { get; set; } = "";
            public bool IsActive { get; set; }
        }
    }
}