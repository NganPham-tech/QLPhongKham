using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.Services;
using QLPhongKham.ViewModels;

[Area("Admin")]
[Authorize(Roles = "Admin")]
public class UserManagementController : Controller
{
    private readonly IUserService _userService;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ApplicationDbContext _context;

    public UserManagementController(
        IUserService userService,
        RoleManager<IdentityRole> roleManager,
        UserManager<IdentityUser> userManager, 
        ApplicationDbContext context)
    {
        _userService = userService;
        _roleManager = roleManager;
        _userManager = userManager;
        _context = context;
    }

    public async Task<IActionResult> Index()
    {
        var users = await _userService.GetAllUsersAsync();
        var userRoleViewModel = new List<UserRoleViewModel>();

        foreach (var user in users)
        {
            var roles = await _userService.GetUserRolesAsync(user);
            userRoleViewModel.Add(new UserRoleViewModel
            {
                User = user,
                Roles = roles
            });
        }

        return View(userRoleViewModel);
    }

    [HttpGet]
    public async Task<IActionResult> AssignRole(string userId)
    {
        var user = await _userService.GetUserByIdAsync(userId);
        if (user == null) return NotFound();        var model = new AssignRoleViewModel
        {
            User = user,
            AvailableRoles = _roleManager.Roles.Select(r => r.Name).Where(n => n != null).Select(n => n!).ToList(),
            UserRoles = await _userService.GetUserRolesAsync(user)
        };

        return View(model);
    }

    [HttpPost]
    public async Task<IActionResult> AssignRole(AssignRoleViewModel model)
    {
        var user = await _userService.GetUserByIdAsync(model.User.Id);
        if (user == null) return NotFound();

        // Lấy roles hiện tại
        var currentRoles = await _userService.GetUserRolesAsync(user);

        // Remove all current roles
        foreach (var role in currentRoles)
        {
            await _userService.RemoveRoleAsync(user, role);
        }

        // Add selected roles và tạo records tương ứng
        if (model.SelectedRoles != null)
        {
            foreach (var role in model.SelectedRoles)
            {
                await _userService.AssignRoleAsync(user, role);

                // Tự động tạo Doctor/Staff record khi phân quyền
                await CreateCorrespondingRecord(user, role, currentRoles);
            }
        }

        TempData["Success"] = "Phân quyền thành công!";
        return RedirectToAction(nameof(Index));
    }

    private async Task CreateCorrespondingRecord(IdentityUser user, string newRole, IList<string> oldRoles)
    {
        try
        {
            switch (newRole)
            {
                case "BacSi":
                    // Chỉ tạo Doctor record nếu chưa có và không phải là role cũ
                    if (!oldRoles.Contains("BacSi"))
                    {
                        var existingDoctor = _context.Doctors.FirstOrDefault(d => d.Email == user.Email);
                        if (existingDoctor == null)
                        {                            var doctor = new Doctor
                            {
                                FirstName = "Bác sĩ", // Có thể lấy từ form hoặc để admin điền sau
                                LastName = user.UserName?.Split('@')[0] ?? "Mới",
                                Gender = "Nam", // Default
                                Email = user.Email ?? string.Empty,
                                Phone = "Chưa cập nhật",
                                Specialty = "Đa khoa",
                                Address = "Chưa cập nhật",
                                Qualification = "Chưa cập nhật",
                                Description = "Tài khoản được tạo bởi Admin"
                            };

                            _context.Doctors.Add(doctor);
                            await _context.SaveChangesAsync();
                        }
                    }
                    break;

                case "NhanVien":
                    // Chỉ tạo Staff record nếu chưa có và không phải là role cũ
                    if (!oldRoles.Contains("NhanVien"))
                    {
                        var existingStaff = _context.Staffs.FirstOrDefault(s => s.Email == user.Email);
                        if (existingStaff == null)
                        {                            var staff = new Staff
                            {
                                FirstName = "Nhân viên",
                                LastName = user.UserName?.Split('@')[0] ?? "Mới",
                                Gender = "Nam", // Default
                                Email = user.Email ?? string.Empty,
                                Phone = "Chưa cập nhật",
                                Position = "Nhân viên",
                                Department = "Tổng hợp",
                                Address = "Chưa cập nhật",
                                Salary = 8000000 
                            };

                            _context.Staffs.Add(staff);
                            await _context.SaveChangesAsync();
                        }
                    }
                    break;

                case "BenhNhan":
                    // Tạo Patient record nếu chưa có
                    if (!oldRoles.Contains("BenhNhan"))
                    {
                        var existingPatient = _context.Patients.FirstOrDefault(p => p.Email == user.Email);
                        if (existingPatient == null)
                        {                            var patient = new Patient
                            {
                                FirstName = "Bệnh nhân",
                                LastName = user.UserName?.Split('@')[0] ?? "Mới",
                                Gender = "Nam", // Default
                                Email = user.Email ?? string.Empty,
                                Phone = "Chưa cập nhật",
                                Address = "Chưa cập nhật",
                                DateOfBirth = DateTime.Now.AddYears(-30) // Default age 30
                            };

                            _context.Patients.Add(patient);
                            await _context.SaveChangesAsync();
                        }
                    }
                    break;
            }
        }
        catch (Exception ex)
        {
            // Log error nhưng không làm fail toàn bộ quá trình
            TempData["Warning"] = $"Phân quyền thành công nhưng có lỗi tạo record: {ex.Message}";
        }
    }

    // Action để tạo tài khoản mới cho staff (không cần user đăng ký trước)
    [HttpGet]
    public IActionResult CreateStaffAccount()
    {
        ViewBag.AvailableRoles = new List<string> { "BacSi", "NhanVien" };
        return View(new CreateStaffAccountViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> CreateStaffAccount(CreateStaffAccountViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                // Tạo IdentityUser
                var user = new IdentityUser
                {
                    UserName = model.Email,
                    Email = model.Email,
                    EmailConfirmed = true
                };

                // Sử dụng _userManager trực tiếp thay vì _userService.GetUserManager()
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Gán role
                    await _userManager.AddToRoleAsync(user, model.Role);                    // Tạo record tương ứng
                    if (model.Role == "BacSi")
                    {                        var doctor = new Doctor
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            DateOfBirth = model.DateOfBirth,
                            Gender = model.Gender,
                            Email = model.Email ?? string.Empty,
                            Phone = model.Phone ?? "Chưa cập nhật",
                            Specialty = model.Specialty ?? "Đa khoa",
                            Address = model.Address ?? "Chưa cập nhật",
                            Qualification = model.Qualification ?? "Bác sĩ",
                            Description = model.Description ?? "Không có mô tả",
                            DateHired = model.DateHired,
                            PhotoUrl = "/images/default-doctor.jpg"
                        };

                        _context.Doctors.Add(doctor);
                    }
                    else if (model.Role == "NhanVien")
                    {                        var staff = new Staff
                        {
                            FirstName = model.FirstName,
                            LastName = model.LastName,
                            DateOfBirth = model.DateOfBirth,
                            Gender = model.Gender,
                            Email = model.Email ?? string.Empty,
                            Phone = model.Phone ?? "Chưa cập nhật",
                            Position = model.Position ?? "Nhân viên",
                            Department = model.Department ?? "Tổng hợp",
                            Address = model.Address ?? "Chưa cập nhật",
                            Salary = model.Salary ?? 8000000,
                            DateHired = model.DateHired
                        };

                        _context.Staffs.Add(staff);
                    }

                    await _context.SaveChangesAsync();

                    TempData["Success"] = $"Tạo tài khoản {model.Role} thành công!";
                    return RedirectToAction(nameof(Index));
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError("", error.Description);
                    }
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }
        }

        ViewBag.AvailableRoles = new List<string> { "BacSi", "NhanVien" };
        return View(model);
    }
}
