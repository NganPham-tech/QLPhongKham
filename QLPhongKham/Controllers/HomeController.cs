using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Models;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace QLPhongKham.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly UserManager<IdentityUser> _userManager;

    public HomeController(ILogger<HomeController> logger, UserManager<IdentityUser> userManager)
    {
        _logger = logger;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index()
    {
        // Kiểm tra nếu người dùng đã đăng nhập
        if (User.Identity?.IsAuthenticated == true)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user != null)
            {
                var roles = await _userManager.GetRolesAsync(user);
                  // Điều hướng dựa trên role
                if (roles.Contains("Admin"))
                {
                    // Sử dụng LocalRedirect với đường dẫn tuyệt đối để tránh vòng lặp
                    return LocalRedirect("/Admin/Home/Index");
                }
                else if (roles.Contains("BacSi"))
                {
                    return RedirectToAction("Index", "Home", new { area = "BacSi" });
                }
                else if (roles.Contains("NhanVien"))
                {
                    return RedirectToAction("Index", "Home", new { area = "NhanVien" });
                }
                // Các role khác hoặc BenhNhan sẽ về trang Patient
            }
        }
        
        // Redirect to Patient Home cho người dùng chưa đăng nhập hoặc BenhNhan
        return RedirectToAction("Index", "PatientHome");
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
