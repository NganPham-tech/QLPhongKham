using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Identity;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.Services;

namespace QLPhongKham.Controllers
{    public class PatientHomeController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public PatientHomeController(ApplicationDbContext context, INotificationService notificationService, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _notificationService = notificationService;
            _userManager = userManager;
        }        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu cho trang chủ bệnh nhân
            var featuredDoctors = await _context.Doctors
                .Where(d => d.IsActive)
                .OrderBy(d => d.FirstName)
                .Take(3)
                .ToListAsync();

            var featuredServices = await _context.Services
                .Where(s => s.IsActive)
                .OrderBy(s => s.Name)
                .Take(6)
                .ToListAsync();

            ViewBag.FeaturedDoctors = featuredDoctors;
            ViewBag.FeaturedServices = featuredServices;

            // Lấy thông báo cho user hiện tại nếu đã đăng nhập
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var unreadCount = await _notificationService.GetUnreadCountAsync(userId);
                    var recentNotifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);
                    
                    ViewBag.UnreadNotificationCount = unreadCount;
                    ViewBag.RecentNotifications = recentNotifications.Take(3).ToList();
                }
            }

            return View();
        }
    }
}
