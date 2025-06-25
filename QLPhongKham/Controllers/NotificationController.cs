using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using QLPhongKham.Services;

namespace QLPhongKham.Controllers
{
    [Authorize(Roles = "BenhNhan")]
    public class NotificationController : Controller
    {
        private readonly INotificationService _notificationService;
        private readonly UserManager<IdentityUser> _userManager;

        public NotificationController(INotificationService notificationService, UserManager<IdentityUser> userManager)
        {
            _notificationService = notificationService;
            _userManager = userManager;
        }

        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Unauthorized();

            var notifications = await _notificationService.GetUserNotificationsAsync(userId);
            return View(notifications);
        }

        [HttpPost]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            await _notificationService.MarkAsReadAsync(id);
            return Json(new { success = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetUnreadCount()
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { count = 0 });

            var count = await _notificationService.GetUnreadCountAsync(userId);
            return Json(new { count });
        }

        [HttpGet]
        public async Task<IActionResult> GetLatest(int take = 5)
        {
            var userId = _userManager.GetUserId(User);
            if (userId == null) return Json(new { notifications = new List<object>() });

            var notifications = await _notificationService.GetUserNotificationsAsync(userId, unreadOnly: true);
            
            var result = notifications.Take(take).Select(n => new
            {
                id = n.Id,
                title = n.Title,
                message = n.Message,
                type = n.Type,
                createdAt = n.CreatedAt.ToString("dd/MM/yyyy HH:mm"),
                relatedUrl = n.RelatedUrl
            });

            return Json(new { notifications = result });
        }
    }
}
