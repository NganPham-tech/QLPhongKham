using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class StaffController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public StaffController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Admin/Staff
        public async Task<IActionResult> Index()
        {
            var staffs = await _context.Staffs.ToListAsync();
            return View(staffs);
        }

        // GET: Admin/Staff/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs
                .FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // GET: Admin/Staff/Create
        public IActionResult Create()
        {
            return View();
        }        // POST: Admin/Staff/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("FirstName,LastName,DateOfBirth,Gender,Email,Phone,Position,Department,Address,Salary")] Staff staff)
        {
            try
            {
                // Set default DateHired to today
                staff.DateHired = DateTime.Today;

                // Validate DateOfBirth
                if (staff.DateOfBirth == default(DateTime))
                {
                    ModelState.AddModelError("DateOfBirth", "Vui lòng chọn ngày sinh.");
                }
                else if (staff.DateOfBirth > DateTime.Today.AddYears(-16))
                {
                    ModelState.AddModelError("DateOfBirth", "Nhân viên phải từ 16 tuổi trở lên.");
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(staff.Email))
                {
                    ModelState.AddModelError("Email", "Email là bắt buộc.");
                }

                if (ModelState.IsValid)
                {
                    // Kiểm tra email đã tồn tại chưa
                    var existingStaff = await _context.Staffs.FirstOrDefaultAsync(s => s.Email == staff.Email);
                    if (existingStaff != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi nhân viên khác.");
                        return View(staff);
                    }

                    // Tạo tài khoản Identity nếu chưa có
                    var user = await _userManager.FindByEmailAsync(staff.Email!);
                    if (user == null)
                    {
                        user = new IdentityUser
                        {
                            UserName = staff.Email,
                            Email = staff.Email,
                            EmailConfirmed = true
                        };

                        // Tạo mật khẩu mặc định
                        string defaultPassword = "Staff@123456";
                        var result = await _userManager.CreateAsync(user, defaultPassword);
                        
                        if (result.Succeeded)
                        {
                            // Gán role NhanVien
                            await _userManager.AddToRoleAsync(user, "NhanVien");
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            return View(staff);
                        }
                    }

                    _context.Add(staff);
                    await _context.SaveChangesAsync();
                    
                    TempData["Success"] = $"Thêm nhân viên {staff.FullName} thành công! Tài khoản đăng nhập: {staff.Email} / Mật khẩu: Staff@123456";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
            }
            
            return View(staff);
        }

        // GET: Admin/Staff/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs.FindAsync(id);
            if (staff == null)
            {
                return NotFound();
            }
            return View(staff);
        }

        // POST: Admin/Staff/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("StaffId,FirstName,LastName,DateOfBirth,Gender,Email,Phone,Position,Department,Address,Salary,DateHired")] Staff staff)
        {
            if (id != staff.StaffId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra email đã tồn tại ở nhân viên khác chưa
                    var existingStaff = await _context.Staffs
                        .FirstOrDefaultAsync(s => s.Email == staff.Email && s.StaffId != staff.StaffId);
                    if (existingStaff != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi nhân viên khác.");
                        return View(staff);
                    }

                    _context.Update(staff);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin nhân viên thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!StaffExists(staff.StaffId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                    return View(staff);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(staff);
        }

        // GET: Admin/Staff/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var staff = await _context.Staffs
                .FirstOrDefaultAsync(m => m.StaffId == id);
            if (staff == null)
            {
                return NotFound();
            }

            return View(staff);
        }

        // POST: Admin/Staff/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var staff = await _context.Staffs.FindAsync(id);
                if (staff != null)
                {                    // Xóa tài khoản Identity liên quan (tùy chọn)
                    var user = await _userManager.FindByEmailAsync(staff.Email!);
                    if (user != null)
                    {
                        // Cảnh báo: Điều này sẽ xóa hoàn toàn tài khoản đăng nhập
                        // Có thể bạn muốn chỉ disable thay vì xóa
                        await _userManager.DeleteAsync(user);
                    }

                    _context.Staffs.Remove(staff);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa nhân viên thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa nhân viên: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool StaffExists(int id)
        {
            return _context.Staffs.Any(e => e.StaffId == id);
        }
    }
}
