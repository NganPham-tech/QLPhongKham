using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AllowAnonymous]
    public class UserRoleController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public UserRoleController(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<IActionResult> AssignAdminRole(string email = "admin@phongkham.com")
        {
            try
            {
                // Tìm user theo email
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Json(new { success = false, message = $"User with email {email} not found" });
                }

                // Kiểm tra role Admin có tồn tại không
                if (!await _roleManager.RoleExistsAsync("Admin"))
                {
                    await _roleManager.CreateAsync(new IdentityRole("Admin"));
                }

                // Assign role Admin cho user
                if (!await _userManager.IsInRoleAsync(user, "Admin"))
                {
                    var result = await _userManager.AddToRoleAsync(user, "Admin");
                    if (result.Succeeded)
                    {
                        return Json(new { success = true, message = $"Admin role assigned to {email} successfully" });
                    }
                    else
                    {
                        return Json(new { success = false, message = "Failed to assign admin role", errors = result.Errors });
                    }
                }
                else
                {
                    return Json(new { success = true, message = $"User {email} already has Admin role" });
                }
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> GetUserInfo(string email = "admin@phongkham.com")
        {
            try
            {
                var user = await _userManager.FindByEmailAsync(email);
                if (user == null)
                {
                    return Json(new { success = false, message = "User not found" });
                }

                var roles = await _userManager.GetRolesAsync(user);
                return Json(new { 
                    success = true, 
                    user = new { 
                        Id = user.Id, 
                        Email = user.Email, 
                        UserName = user.UserName 
                    }, 
                    roles = roles 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }

        public async Task<IActionResult> ListAllUsers()
        {
            try
            {
                var users = await _userManager.Users.ToListAsync();
                var userList = new List<object>();

                foreach (var user in users)
                {
                    var roles = await _userManager.GetRolesAsync(user);
                    userList.Add(new
                    {
                        Id = user.Id,
                        Email = user.Email,
                        UserName = user.UserName,
                        Roles = roles
                    });
                }

                return Json(new { success = true, users = userList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Error: " + ex.Message });
            }
        }
    }
}
