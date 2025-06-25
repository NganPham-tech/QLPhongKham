using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class MedicineController : Controller
    {
        private readonly ApplicationDbContext _context;

        public MedicineController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Medicine
        public async Task<IActionResult> Index()
        {
            var medicines = await _context.Medicines.ToListAsync();
            return View(medicines);
        }

        // GET: Admin/Medicine/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.MedicineId == id);
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        // GET: Admin/Medicine/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Admin/Medicine/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Name,Description,UnitPrice,Unit,Manufacturer,IsActive")] Medicine medicine)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên thuốc đã tồn tại chưa
                    var existingMedicine = await _context.Medicines.FirstOrDefaultAsync(m => m.Name == medicine.Name);
                    if (existingMedicine != null)
                    {
                        ModelState.AddModelError("Name", "Tên thuốc này đã tồn tại.");
                        return View(medicine);
                    }

                    _context.Add(medicine);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Thêm thuốc thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Có lỗi xảy ra: {ex.Message}");
                }
            }
            return View(medicine);
        }

        // GET: Admin/Medicine/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines.FindAsync(id);
            if (medicine == null)
            {
                return NotFound();
            }
            return View(medicine);
        }

        // POST: Admin/Medicine/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("MedicineId,Name,Description,UnitPrice,Unit,Manufacturer,IsActive")] Medicine medicine)
        {
            if (id != medicine.MedicineId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Kiểm tra tên thuốc đã tồn tại ở thuốc khác chưa
                    var existingMedicine = await _context.Medicines
                        .FirstOrDefaultAsync(m => m.Name == medicine.Name && m.MedicineId != medicine.MedicineId);
                    if (existingMedicine != null)
                    {
                        ModelState.AddModelError("Name", "Tên thuốc này đã tồn tại.");
                        return View(medicine);
                    }

                    _context.Update(medicine);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật thuốc thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MedicineExists(medicine.MedicineId))
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
                    return View(medicine);
                }
                return RedirectToAction(nameof(Index));
            }
            return View(medicine);
        }

        // GET: Admin/Medicine/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var medicine = await _context.Medicines
                .FirstOrDefaultAsync(m => m.MedicineId == id);
            if (medicine == null)
            {
                return NotFound();
            }

            return View(medicine);
        }

        // POST: Admin/Medicine/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var medicine = await _context.Medicines.FindAsync(id);
                if (medicine != null)
                {
                    // Kiểm tra xem thuốc có được sử dụng trong kho hay hóa đơn nào không
                    var hasInventory = await _context.Inventories.AnyAsync(i => i.MedicineId == id);
                    var hasInvoiceDetails = await _context.InvoiceDetails.AnyAsync(i => i.MedicineId == id);
                    
                    if (hasInventory || hasInvoiceDetails)
                    {
                        TempData["Error"] = "Không thể xóa thuốc này vì đã có trong kho hoặc đã được sử dụng.";
                        return RedirectToAction(nameof(Index));
                    }

                    _context.Medicines.Remove(medicine);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Xóa thuốc thành công!";
                }
            }
            catch (Exception ex)
            {
                TempData["Error"] = $"Không thể xóa thuốc: {ex.Message}";
            }
            
            return RedirectToAction(nameof(Index));
        }

        private bool MedicineExists(int id)
        {
            return _context.Medicines.Any(e => e.MedicineId == id);
        }
    }
}
