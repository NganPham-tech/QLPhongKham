using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class ServiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public ServiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Service
        public async Task<IActionResult> Index()
        {
            var services = await _context.Services.ToListAsync();
            return View(services);
        }

        // GET: Admin/Service/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // GET: Admin/Service/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Service/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,Price,Duration,IsActive")] Service service)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên dịch vụ đã tồn tại chưa
                    var existingService = await _context.Services.FirstOrDefaultAsync(s => s.Name == service.Name);
                    if (existingService != null)
                    {
                        ModelState.AddModelError("Name", "Tên dịch vụ này đã tồn tại.");
                        return View(service);
                    }

                    _context.Add(service);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm dịch vụ thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            return View(service);
        }

        // GET: Admin/Service/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services.FindAsync(id);
            if (service == null)
            {
                return NotFound();
            }
            return View(service);
        }

        // POST: Admin/Service/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("ServiceId,Name,Description,Price,Duration,IsActive")] Service service)
        {
            if (id != service.ServiceId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên dịch vụ đã tồn tại ở dịch vụ khác chưa
                    var existingService = await _context.Services
                        .FirstOrDefaultAsync(s => s.Name == service.Name && s.ServiceId != service.ServiceId);
                    if (existingService != null)
                    {
                        ModelState.AddModelError("Name", "Tên dịch vụ này đã tồn tại.");
                        return View(service);
                    }

                    _context.Update(service);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật dịch vụ thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ServiceExists(service.ServiceId))
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
                    return View(service);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(service);
        }

        // GET: Admin/Service/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var service = await _context.Services
                .FirstOrDefaultAsync(m => m.ServiceId == id);
            if (service == null)
            {
                return NotFound();
            }

            return View(service);
        }

        // POST: Admin/Service/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var service = await _context.Services.FindAsync(id);
                if (service != null)
                {
                    // Kiểm tra xem dịch vụ có được sử dụng trong cuộc hẹn nào không
                    var hasAppointments = await _context.Appointments.AnyAsync(a => a.ServiceId == id);
                    if (hasAppointments)
                    {
                        TempData["Error"] = "Không thể xóa dịch vụ này vì đã có cuộc hẹn sử dụng.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Services.Remove(service);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa dịch vụ thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa dịch vụ: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool ServiceExists(int id)
        {
            return _context.Services.Any(e => e.ServiceId == id);
        }
    }
}
