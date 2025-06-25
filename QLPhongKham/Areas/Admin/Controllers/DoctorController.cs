using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.ViewModels;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class DoctorController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public DoctorController(ApplicationDbContext context, UserManager<IdentityUser> userManager, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: Admin/Doctor
        public async Task<IActionResult> Index()
        {
            var doctors = await _context.Doctors.ToListAsync();
            return View(doctors);
        }

        // GET: Admin/Doctor/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // GET: Admin/Doctor/Create
        public IActionResult Create()
        {
            ViewBag.Specialties = GetSpecialties();
            return View(new DoctorCreateViewModel());
        }

        // POST: Admin/Doctor/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(DoctorCreateViewModel model)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra email đã tồn tại chưa
                    var existingDoctor = await _context.Doctors.FirstOrDefaultAsync(d => d.Email == model.Email);
                    if (existingDoctor != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi bác sĩ khác.");
                        ViewBag.Specialties = GetSpecialties();
                        return View(model);
                    }

                    // Xử lý upload hình ảnh
                    string photoUrl = "/images/default-doctor.jpg";
                    if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                    {
                        photoUrl = await SavePhotoAsync(model.PhotoFile);
                    }                    // Tạo Doctor entity
                    var doctor = new Doctor
                    {
                        FirstName = model.FirstName,
                        LastName = model.LastName,
                        DateOfBirth = model.DateOfBirth,
                        Gender = model.Gender,
                        Email = model.Email,
                        Phone = model.Phone,
                        Specialty = model.Specialty,
                        Address = model.Address,
                        Qualification = model.Qualification,
                        Description = model.Description,
                        PhotoUrl = photoUrl,
                        DateHired = model.DateHired
                    };

                    // Tạo tài khoản Identity nếu chưa có
                    var user = await _userManager.FindByEmailAsync(doctor.Email!);
                    if (user == null)
                    {
                        user = new IdentityUser
                        {
                            UserName = doctor.Email,
                            Email = doctor.Email,
                            EmailConfirmed = true
                        };

                        // Tạo mật khẩu mặc định
                        string defaultPassword = "Doctor@123456";
                        var result = await _userManager.CreateAsync(user, defaultPassword);
                        
                        if (result.Succeeded)
                        {
                            // Gán role BacSi
                            await _userManager.AddToRoleAsync(user, "BacSi");
                        }
                        else
                        {
                            foreach (var error in result.Errors)
                            {
                                ModelState.AddModelError("", error.Description);
                            }
                            ViewBag.Specialties = GetSpecialties();
                            return View(model);
                        }
                    }

                    _context.Add(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm bác sĩ thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            
            ViewBag.Specialties = GetSpecialties();
            return View(model);
        }

        // GET: Admin/Doctor/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors.FindAsync(id);
            if (doctor == null)
            {
                return NotFound();
            }            var model = new DoctorEditViewModel
            {
                DoctorId = doctor.DoctorId,
                FirstName = doctor.FirstName,
                LastName = doctor.LastName,
                DateOfBirth = doctor.DateOfBirth,
                Gender = doctor.Gender,
                Email = doctor.Email ?? "",
                Phone = doctor.Phone,
                Specialty = doctor.Specialty,
                Address = doctor.Address,
                Qualification = doctor.Qualification,
                Description = doctor.Description,
                PhotoUrl = doctor.PhotoUrl,
                DateHired = doctor.DateHired
            };
            
            ViewBag.Specialties = GetSpecialties();
            return View(model);
        }

        // POST: Admin/Doctor/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, DoctorEditViewModel model)
        {
            if (id != model.DoctorId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var doctor = await _context.Doctors.FindAsync(id);
                    if (doctor == null)
                    {
                        return NotFound();
                    }

                    // Kiểm tra email đã tồn tại ở bác sĩ khác chưa
                    var existingDoctor = await _context.Doctors
                        .FirstOrDefaultAsync(d => d.Email == model.Email && d.DoctorId != model.DoctorId);
                    if (existingDoctor != null)
                    {
                        ModelState.AddModelError("Email", "Email này đã được sử dụng bởi bác sĩ khác.");
                        ViewBag.Specialties = GetSpecialties();
                        return View(model);
                    }

                    // Xử lý upload hình ảnh mới
                    if (model.PhotoFile != null && model.PhotoFile.Length > 0)
                    {
                        // Xóa ảnh cũ nếu không phải ảnh mặc định
                        if (!string.IsNullOrEmpty(doctor.PhotoUrl) && doctor.PhotoUrl != "/images/default-doctor.jpg")
                        {
                            await DeletePhotoAsync(doctor.PhotoUrl);
                        }
                        
                        model.PhotoUrl = await SavePhotoAsync(model.PhotoFile);
                    }                    // Cập nhật thông tin
                    doctor.FirstName = model.FirstName;
                    doctor.LastName = model.LastName;
                    doctor.DateOfBirth = model.DateOfBirth;
                    doctor.Gender = model.Gender;
                    doctor.Email = model.Email;
                    doctor.Phone = model.Phone;
                    doctor.Specialty = model.Specialty;
                    doctor.Address = model.Address;
                    doctor.Qualification = model.Qualification;
                    doctor.Description = model.Description;
                    doctor.DateHired = model.DateHired;
                    
                    if (!string.IsNullOrEmpty(model.PhotoUrl))
                    {
                        doctor.PhotoUrl = model.PhotoUrl;
                    }

                    _context.Update(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thông tin bác sĩ thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!DoctorExists(model.DoctorId))
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
                    ViewBag.Specialties = GetSpecialties();
                    return View(model);
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Specialties = GetSpecialties();
            return View(model);
        }

        // GET: Admin/Doctor/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var doctor = await _context.Doctors
                .FirstOrDefaultAsync(m => m.DoctorId == id);
            if (doctor == null)
            {
                return NotFound();
            }

            return View(doctor);
        }

        // POST: Admin/Doctor/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var doctor = await _context.Doctors.FindAsync(id);
                if (doctor != null)
                {
                    // Kiểm tra xem bác sĩ có cuộc hẹn nào không
                    var hasAppointments = await _context.Appointments.AnyAsync(a => a.DoctorId == id);
                    if (hasAppointments)
                    {
                        TempData["Error"] = "Không thể xóa bác sĩ này vì đã có cuộc hẹn được đặt.";
                        return RedirectToAction(nameof(Index));
                    }

                    // Xóa ảnh nếu không phải ảnh mặc định
                    if (!string.IsNullOrEmpty(doctor.PhotoUrl) && doctor.PhotoUrl != "/images/default-doctor.jpg")
                    {
                        await DeletePhotoAsync(doctor.PhotoUrl);
                    }

                    // Xóa tài khoản Identity liên quan (tùy chọn)
                    var user = await _userManager.FindByEmailAsync(doctor.Email!);
                    if (user != null)
                    {
                        await _userManager.DeleteAsync(user);
                    }

                    _context.Doctors.Remove(doctor);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa bác sĩ thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa bác sĩ: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool DoctorExists(int id)
        {
            return _context.Doctors.Any(e => e.DoctorId == id);
        }

        private List<string> GetSpecialties()
        {
            return new List<string>
            {
                "Đa khoa",
                "Tim mạch",
                "Thần kinh",
                "Nhi khoa",
                "Sản phụ khoa",
                "Mắt",
                "Tai mũi họng",
                "Da liễu",
                "Răng hàm mặt",
                "Chỉnh hình",
                "Tiêu hóa",
                "Hô hấp",
                "Thận - Tiết niệu",
                "Nội tiết",
                "Ung bướu",
                "Tâm thần",
                "Vật lý trị liệu",
                "Y học cổ truyền"
            };
        }

        private async Task<string> SavePhotoAsync(IFormFile photoFile)
        {
            try
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "doctors");
                
                // Tạo thư mục nếu chưa tồn tại
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Tạo tên file unique
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(photoFile.FileName);
                var filePath = Path.Combine(uploadsFolder, fileName);

                // Lưu file
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await photoFile.CopyToAsync(fileStream);
                }

                return $"/images/doctors/{fileName}";
            }
            catch (Exception)
            {
                return "/images/default-doctor.jpg";
            }
        }

        private async Task DeletePhotoAsync(string photoUrl)
        {
            try
            {
                if (!string.IsNullOrEmpty(photoUrl) && photoUrl.StartsWith("/images/doctors/"))
                {
                    var fileName = Path.GetFileName(photoUrl);
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "images", "doctors", fileName);                    if (System.IO.File.Exists(filePath))
                    {
                        await Task.Run(() => System.IO.File.Delete(filePath));
                    }
                }
            }
            catch (Exception)
            {
                // Log error but don't throw
            }
        }
    }
}
