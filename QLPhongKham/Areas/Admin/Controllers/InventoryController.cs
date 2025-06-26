using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;
using Microsoft.AspNetCore.Authorization;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin")]
    public class InventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Admin/Inventory
        public async Task<IActionResult> Index()
        {
            var inventories = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.ManagedByStaff)
                .OrderBy(i => i.Medicine!.Name)
                .ToListAsync();
            
            return View(inventories);
        }

        // GET: Admin/Inventory/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.ManagedByStaff)
                .FirstOrDefaultAsync(m => m.InventoryId == id);
            
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // GET: Admin/Inventory/Create
        public IActionResult Create(int? medicineId)
        {
            ViewBag.Medicines = _context.Medicines
                .Where(m => !_context.Inventories.Any(i => i.MedicineId == m.MedicineId))
                .ToList();
            
            var model = new Inventory();
            
            if (medicineId.HasValue)
            {
                model.MedicineId = medicineId.Value;
                ViewBag.SelectedMedicineId = medicineId.Value;
            }
            
            // Lấy thông tin user hiện tại để hiển thị
            var currentUserEmail = User.Identity?.Name;
            ViewBag.CurrentUserEmail = currentUserEmail;
            
            return View(model);
        }

        // POST: Admin/Inventory/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedicineId,QuantityInStock,ExpirationDate,Notes")] Inventory inventory)
        {
            // Tự động tìm staff dựa trên user đang đăng nhập
            var currentUserEmail = User.Identity?.Name;
            var currentStaff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.Email == currentUserEmail);
            
            if (currentStaff == null)
            {
                // Nếu không tìm thấy staff, sử dụng staff đầu tiên trong hệ thống
                currentStaff = await _context.Staffs.FirstAsync();
            }
            
            inventory.ManagedByStaffId = currentStaff.StaffId;
            
            // Kiểm tra validation cơ bản trước
            if (inventory.MedicineId <= 0)
            {
                ModelState.AddModelError("MedicineId", "Vui lòng chọn thuốc");
            }
            
            // Kiểm tra thuốc đã có trong kho chưa
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.MedicineId == inventory.MedicineId);
            if (existingInventory != null)
            {
                ModelState.AddModelError("MedicineId", "Thuốc này đã có trong kho");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Load navigation properties để thỏa mãn required
                    var medicine = await _context.Medicines.FindAsync(inventory.MedicineId);
                    
                    if (medicine == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thuốc được chọn!";
                        ViewBag.Medicines = _context.Medicines
                            .Where(m => !_context.Inventories.Any(i => i.MedicineId == m.MedicineId))
                            .ToList();
                        ViewBag.CurrentUserEmail = currentUserEmail;
                        return View(inventory);
                    }
                    
                    // Tạo inventory mới với navigation properties
                    var newInventory = new Inventory
                    {
                        MedicineId = inventory.MedicineId,
                        QuantityInStock = inventory.QuantityInStock,
                        ExpirationDate = inventory.ExpirationDate,
                        ManagedByStaffId = currentStaff.StaffId,
                        Notes = inventory.Notes,
                        ImportDate = DateTime.Now
                    };
                    
                    _context.Add(newInventory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm tồn kho thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu: " + ex.Message;
                }
            }
            
            // Nếu có lỗi, load lại data cho form
            ViewBag.Medicines = _context.Medicines
                .Where(m => !_context.Inventories.Any(i => i.MedicineId == m.MedicineId))
                .ToList();
            ViewBag.CurrentUserEmail = currentUserEmail;
            return View(inventory);
        }

        // GET: Admin/Inventory/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.ManagedByStaff)
                .FirstOrDefaultAsync(i => i.InventoryId == id);
            
            if (inventory == null)
            {
                return NotFound();
            }
            
            return View(inventory);
        }

        // POST: Admin/Inventory/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("InventoryId,MedicineId,QuantityInStock,ExpirationDate,ManagedByStaffId,ImportDate,Notes")] Inventory inventory)
        {
            if (id != inventory.InventoryId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(inventory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Cập nhật tồn kho thành công!";
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!InventoryExists(inventory.InventoryId))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            
            ViewBag.Staff = _context.Staffs.ToList();
            return View(inventory);
        }

        // GET: Admin/Inventory/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.ManagedByStaff)
                .FirstOrDefaultAsync(m => m.InventoryId == id);
            
            if (inventory == null)
            {
                return NotFound();
            }

            return View(inventory);
        }

        // POST: Admin/Inventory/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory != null)
            {
                _context.Inventories.Remove(inventory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Xóa tồn kho thành công!";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Inventory/AdjustStock/5
        public async Task<IActionResult> AdjustStock(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var inventory = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.ManagedByStaff)
                .FirstOrDefaultAsync(i => i.InventoryId == id);
            
            if (inventory == null)
            {
                return NotFound();
            }

            ViewBag.CurrentQuantity = inventory.QuantityInStock;
            return View(inventory);
        }

        // POST: Admin/Inventory/AdjustStock/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AdjustStock(int id, int adjustmentQuantity, string adjustmentType, string reason)
        {
            var inventory = await _context.Inventories.FindAsync(id);
            if (inventory == null)
            {
                return NotFound();
            }

            var oldQuantity = inventory.QuantityInStock;
            
            if (adjustmentType == "increase")
            {
                inventory.QuantityInStock += adjustmentQuantity;
            }
            else if (adjustmentType == "decrease")
            {
                inventory.QuantityInStock = Math.Max(0, inventory.QuantityInStock - adjustmentQuantity);
            }
            else if (adjustmentType == "set")
            {
                inventory.QuantityInStock = adjustmentQuantity;
            }

            inventory.Notes = $"Điều chỉnh từ {oldQuantity} thành {inventory.QuantityInStock}. Lý do: {reason}";

            try
            {
                _context.Update(inventory);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Điều chỉnh tồn kho thành công! Số lượng từ {oldQuantity} thành {inventory.QuantityInStock}";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi điều chỉnh tồn kho: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Inventory/StockReport
        public async Task<IActionResult> StockReport()
        {
            var inventories = await _context.Inventories
                .Include(i => i.Medicine)
                .Include(i => i.ManagedByStaff)
                .OrderBy(i => i.Medicine!.Name)
                .ToListAsync();

            var report = new
            {
                TotalItems = inventories.Count,
                LowStockItems = inventories.Where(i => i.QuantityInStock <= 10).ToList(), // Giả sử mức tối thiểu là 10
                ZeroStockItems = inventories.Where(i => i.QuantityInStock == 0).ToList(),
                ExpiredItems = inventories.Where(i => i.ExpirationDate.HasValue && i.ExpirationDate < DateTime.Now).ToList(),
                ExpiringItems = inventories.Where(i => i.ExpirationDate.HasValue && i.ExpirationDate <= DateTime.Now.AddDays(30) && i.ExpirationDate >= DateTime.Now).ToList(),
                TotalQuantity = inventories.Sum(i => i.QuantityInStock),
                AllItems = inventories
            };

            return View(report);
        }

        // GET: Admin/Inventory/TestCreate
        public IActionResult TestCreate()
        {
            var medicines = _context.Medicines
                .Where(m => !_context.Inventories.Any(i => i.MedicineId == m.MedicineId))
                .ToList();
            var staffs = _context.Staffs.ToList();
            
            var result = new
            {
                MedicinesCount = medicines.Count,
                StaffsCount = staffs.Count,
                Medicines = medicines.Select(m => new { m.MedicineId, m.Name }).ToList(),
                Staffs = staffs.Select(s => new { s.StaffId, s.Name }).ToList()
            };
            
            return Json(result);
        }

        private bool InventoryExists(int id)
        {
            return _context.Inventories.Any(e => e.InventoryId == id);
        }
    }

    [Area("Admin")]
    [AllowAnonymous]  // Temporarily allow anonymous access for testing
    public class TestInventoryController : Controller
    {
        private readonly ApplicationDbContext _context;

        public TestInventoryController(ApplicationDbContext context)
        {
            _context = context;
        }

        public IActionResult Create()
        {
            ViewBag.Medicines = _context.Medicines
                .Where(m => !_context.Inventories.Any(i => i.MedicineId == m.MedicineId))
                .ToList();
            
            var model = new Inventory
            {
                Medicine = null!,
                ManagedByStaff = null!
            };
            
            // Lấy thông tin user hiện tại để hiển thị
            var currentUserEmail = User.Identity?.Name ?? "admin@phongkham.com";
            ViewBag.CurrentUserEmail = currentUserEmail;
            
            return View("~/Areas/Admin/Views/Inventory/Create.cshtml", model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("MedicineId,QuantityInStock,ExpirationDate,Notes")] Inventory inventory)
        {
            // Tự động tìm staff dựa trên user đang đăng nhập
            var currentUserEmail = User.Identity?.Name ?? "admin@phongkham.com";
            var currentStaff = await _context.Staffs
                .FirstOrDefaultAsync(s => s.Email == currentUserEmail);
            
            if (currentStaff == null)
            {
                // Nếu không tìm thấy staff, sử dụng staff đầu tiên trong hệ thống
                currentStaff = await _context.Staffs.FirstAsync();
            }
            
            inventory.ManagedByStaffId = currentStaff.StaffId;
            
            // Kiểm tra thuốc đã có trong kho chưa
            var existingInventory = await _context.Inventories
                .FirstOrDefaultAsync(i => i.MedicineId == inventory.MedicineId);
            if (existingInventory != null)
            {
                ModelState.AddModelError("MedicineId", "Thuốc này đã có trong kho");
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    // Load navigation properties để thỏa mãn required
                    var medicine = await _context.Medicines.FindAsync(inventory.MedicineId);
                    var staff = await _context.Staffs.FindAsync(inventory.ManagedByStaffId);
                    
                    if (medicine == null || staff == null)
                    {
                        TempData["ErrorMessage"] = "Không tìm thấy thuốc hoặc nhân viên được chọn!";
                        ViewBag.Medicines = _context.Medicines
                            .Where(m => !_context.Inventories.Any(i => i.MedicineId == m.MedicineId))
                            .ToList();
                        ViewBag.Staff = _context.Staffs.ToList();
                        return View("~/Areas/Admin/Views/Inventory/Create.cshtml", inventory);
                    }
                    
                    // Tạo inventory mới với navigation properties
                    var newInventory = new Inventory
                    {
                        MedicineId = inventory.MedicineId,
                        QuantityInStock = inventory.QuantityInStock,
                        ExpirationDate = inventory.ExpirationDate,
                        ManagedByStaffId = inventory.ManagedByStaffId,
                        Notes = inventory.Notes,
                        ImportDate = DateTime.Now,
                        Medicine = medicine,
                        ManagedByStaff = staff
                    };
                    
                    _context.Add(newInventory);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Thêm tồn kho thành công!";
                    return RedirectToAction("Index", "Inventory");
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra khi lưu: " + ex.Message;
                }
            }
            
            // Nếu có lỗi, load lại data cho form
            ViewBag.Medicines = _context.Medicines
                .Where(m => !_context.Inventories.Any(i => i.MedicineId == m.MedicineId))
                .ToList();
            ViewBag.Staff = _context.Staffs.ToList();
            return View("~/Areas/Admin/Views/Inventory/Create.cshtml", inventory);
        }
    }
}
