using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Services;
using QLPhongKham.ViewModels;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IUserService _userService;

        public HomeController(
            ApplicationDbContext context,
            IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var totalUsers = await _userService.GetAllUsersAsync();
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                
                // Get appointment statistics
                var allAppointments = await _context.Appointments.ToListAsync();
                var todayAppointments = await _context.Appointments
                    .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .ToListAsync();

                var model = new DashboardViewModel
                {
                    TotalUsers = totalUsers.Count,
                    TotalPatients = await _context.Patients.CountAsync(),
                    TotalDoctors = await _context.Doctors.CountAsync(),
                    TotalAppointments = allAppointments.Count,
                    TodayAppointments = todayAppointments.Count,

                    // Appointment status statistics
                    PendingAppointments = allAppointments.Where(a => a.Status == "Pending").Count(),
                    InProgressAppointments = allAppointments.Where(a => a.Status == "In Progress").Count(),
                    CompletedAppointments = allAppointments.Where(a => a.Status == "Completed").Count(),
                    CancelledAppointments = allAppointments.Where(a => a.Status == "Cancelled").Count(),
                    
                    TotalStaff = await _context.Staffs.CountAsync(),
                    
                    // Today's appointments for quick view
                    TodayAppointmentsList = todayAppointments.Select(a => new TodayAppointmentViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentTime = a.AppointmentDate,
                        PatientName = a.Patient.FullName,
                        DoctorName = a.Doctor.FullName,
                        ServiceName = a.Service.Name,
                        Status = a.Status
                    }).OrderBy(a => a.AppointmentTime).ToList(),

                    // Simple mock data for new features
                    NewPatientsToday = 5,
                    NewPatientsThisMonth = 45,
                    TodayRevenue = 2500000,
                    MonthlyRevenue = 85000000,
                    ExpiredMedicines = 3,
                    LowStockMedicines = 7,
                    ExpiringMedicines = 12,
                    LateAppointments = todayAppointments.Where(a => a.AppointmentDate < DateTime.Now && a.Status == "Pending").Count(),
                    
                    // Initialize collections with sample data
                    Alerts = new List<AlertViewModel>
                    {
                        new AlertViewModel
                        {
                            Type = "Medicine",
                            Level = "Warning",
                            Title = "Thuốc sắp hết hạn",
                            Message = "Có 12 loại thuốc sắp hết hạn trong 30 ngày tới",
                            ActionUrl = "/Admin/Medicine",
                            CreatedAt = DateTime.Now
                        },
                        new AlertViewModel
                        {
                            Type = "Inventory",
                            Level = "Danger",
                            Title = "Tồn kho thấp",
                            Message = "Có 7 loại thuốc có tồn kho thấp cần nhập thêm",
                            ActionUrl = "/Admin/Medicine",
                            CreatedAt = DateTime.Now
                        }
                    },
                    
                    MonthlyRevenueChart = new List<MonthlyRevenueData>
                    {
                        new MonthlyRevenueData { Month = "01/2025", Revenue = 75000000, AppointmentCount = 180 },
                        new MonthlyRevenueData { Month = "02/2025", Revenue = 82000000, AppointmentCount = 195 },
                        new MonthlyRevenueData { Month = "03/2025", Revenue = 78000000, AppointmentCount = 175 },
                        new MonthlyRevenueData { Month = "04/2025", Revenue = 89000000, AppointmentCount = 210 },
                        new MonthlyRevenueData { Month = "05/2025", Revenue = 91000000, AppointmentCount = 225 },
                        new MonthlyRevenueData { Month = "06/2025", Revenue = 85000000, AppointmentCount = 200 }
                    },
                    
                    TopServices = new List<ServiceStatistics>
                    {
                        new ServiceStatistics { ServiceName = "Khám tổng quát", Count = 85, Revenue = 25500000, Percentage = 35 },
                        new ServiceStatistics { ServiceName = "Siêu âm", Count = 45, Revenue = 18000000, Percentage = 25 },
                        new ServiceStatistics { ServiceName = "Xét nghiệm máu", Count = 65, Revenue = 13000000, Percentage = 18 },
                        new ServiceStatistics { ServiceName = "X-quang", Count = 30, Revenue = 9000000, Percentage = 12 },
                        new ServiceStatistics { ServiceName = "Khám chuyên khoa", Count = 25, Revenue = 7500000, Percentage = 10 }
                    },
                    
                    AppointmentStatusChart = new List<AppointmentStatusData>
                    {
                        new AppointmentStatusData { Status = "Hoàn thành", Count = allAppointments.Count(a => a.Status == "Completed"), Color = "#28a745" },
                        new AppointmentStatusData { Status = "Đang chờ", Count = allAppointments.Count(a => a.Status == "Pending"), Color = "#ffc107" },
                        new AppointmentStatusData { Status = "Đang tiến hành", Count = allAppointments.Count(a => a.Status == "In Progress"), Color = "#17a2b8" },
                        new AppointmentStatusData { Status = "Đã hủy", Count = allAppointments.Count(a => a.Status == "Cancelled"), Color = "#dc3545" }
                    },

                    // Recent activities (can be extended)
                    RecentActivities = new List<RecentActivityViewModel>
                    {
                        new RecentActivityViewModel
                        {
                            Action = "Tạo lịch hẹn",
                            UserName = "System",
                            Timestamp = DateTime.Now.AddMinutes(-30),
                            Description = "Lịch hẹn mới được tạo trong hệ thống",
                            Icon = "calendar-plus",
                            Color = "success"
                        }
                    },
                    
                    // Mock unpaid invoices
                    UnpaidInvoicesList = new List<UnpaidInvoiceViewModel>
                    {
                        new UnpaidInvoiceViewModel
                        {
                            InvoiceId = 1,
                            InvoiceNumber = "HD001",
                            PatientName = "Nguyễn Văn A",
                            TotalAmount = 500000,
                            RemainingAmount = 200000,
                            CreatedDate = DateTime.Now.AddDays(-5),
                            DueDate = DateTime.Now.AddDays(5),
                            IsOverdue = false
                        }
                    }
                };

                return View(model);
            }
            catch (Exception ex)
            {
                // Log error in production
                var emptyModel = new DashboardViewModel
                {
                    TotalUsers = 0,
                    TotalPatients = 0,
                    TotalDoctors = 0,
                    TotalAppointments = 0,
                    TodayAppointments = 0,
                    PendingAppointments = 0,
                    CompletedAppointments = 0,
                    TotalStaff = 0
                };
                TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu dashboard: " + ex.Message;
                return View(emptyModel);
            }
        }

        // Test action không cần authorization
        [AllowAnonymous]
        [HttpGet]
        [Route("/AdminTest")]
        public IActionResult Test()
        {
            return Content("Admin routing hoạt động! Bạn cần đăng nhập với role Admin để truy cập /Admin");
        }        // GET: Admin (Root route for admin area)
        [HttpGet]
        [Route("/Admin")]
        public async Task<IActionResult> AdminRoot()
        {
            // Gọi trực tiếp logic của Index thay vì redirect để tránh vòng lặp
            try
            {
                var totalUsers = await _userService.GetAllUsersAsync();
                var today = DateTime.Today;
                var tomorrow = today.AddDays(1);
                
                // Get appointment statistics
                var allAppointments = await _context.Appointments.ToListAsync();
                var todayAppointments = await _context.Appointments
                    .Where(a => a.AppointmentDate >= today && a.AppointmentDate < tomorrow)
                    .Include(a => a.Patient)
                    .Include(a => a.Doctor)
                    .Include(a => a.Service)
                    .ToListAsync();

                // Build the dashboard model
                var model = new DashboardViewModel
                {
                    TotalUsers = totalUsers.Count,
                    TotalPatients = await _context.Patients.CountAsync(),
                    TotalDoctors = await _context.Doctors.CountAsync(),
                    TotalAppointments = allAppointments.Count,
                    TodayAppointments = todayAppointments.Count,
                    
                    // Appointment status statistics
                    PendingAppointments = allAppointments.Where(a => a.Status == "Pending").Count(),
                    InProgressAppointments = allAppointments.Where(a => a.Status == "In Progress").Count(),
                    CompletedAppointments = allAppointments.Where(a => a.Status == "Completed").Count(),
                    CancelledAppointments = allAppointments.Where(a => a.Status == "Cancelled").Count(),
                    
                    TotalStaff = await _context.Staffs.CountAsync(),
                    
                    // Today's appointments for quick view
                    TodayAppointmentsList = todayAppointments.Select(a => new TodayAppointmentViewModel
                    {
                        AppointmentId = a.AppointmentId,
                        AppointmentTime = a.AppointmentDate,
                        PatientName = a.Patient.FullName,
                        DoctorName = a.Doctor.FullName,
                        ServiceName = a.Service.Name,
                        Status = a.Status
                    }).OrderBy(a => a.AppointmentTime).ToList()
                };

                return View("Index", model);
            }
            catch (Exception)
            {
                // Log error in production
                var emptyModel = new DashboardViewModel
                {
                    TotalUsers = 0,
                    TotalPatients = 0,
                    TotalDoctors = 0,
                    TotalAppointments = 0,
                    TodayAppointments = 0,
                    PendingAppointments = 0,
                    CompletedAppointments = 0,
                    TotalStaff = 0
                };
                TempData["Error"] = "Có lỗi xảy ra khi tải dữ liệu dashboard.";
                return View("Index", emptyModel);
            }
        }
    }
}
