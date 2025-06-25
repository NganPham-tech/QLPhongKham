using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous]
    public class TestLoginController : Controller
    {
        private readonly SignInManager<IdentityUser> _signInManager;
        private readonly UserManager<IdentityUser> _userManager;

        public TestLoginController(SignInManager<IdentityUser> signInManager, UserManager<IdentityUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }

        // Test action để kiểm tra điều hướng admin
        [HttpGet]
        [Route("/TestAdminLogin")]
        public async Task<IActionResult> TestAdminLogin()
        {
            var adminEmail = "admin@phongkham.com";
            var adminPassword = "Admin@123456";

            var result = await _signInManager.PasswordSignInAsync(
                adminEmail,
                adminPassword,
                isPersistent: false,
                lockoutOnFailure: false);

            if (result.Succeeded)
            {
                // Kiểm tra role
                var user = await _userManager.FindByEmailAsync(adminEmail);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    
                    if (roles.Contains("Admin"))
                    {
                        // Điều hướng đến Admin area
                        return RedirectToAction("Index", "Home", new { area = "Admin" });
                    }
                }
                
                return Content($"Đăng nhập thành công nhưng không có role Admin. Roles: {string.Join(", ", await _userManager.GetRolesAsync(user!))}");
            }

            return Content("Đăng nhập thất bại!");
        }

        // Test hiển thị thông tin user hiện tại
        [HttpGet]
        [Route("/TestCurrentUser")]
        public async Task<IActionResult> TestCurrentUser()
        {
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    return Content($"User: {user.Email}, Roles: {string.Join(", ", roles)}");
                }
            }

            return Content("Người dùng chưa đăng nhập");
        }

        [HttpGet]
        [Route("/TestLogout")]
        public async Task<IActionResult> TestLogout()
        {
            await _signInManager.SignOutAsync();
            return Content("Đã đăng xuất thành công!");
        }
    }
}
