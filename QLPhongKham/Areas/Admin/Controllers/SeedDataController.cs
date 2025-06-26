using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Models;
using QLPhongKham.Data;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class SeedDataController : Controller
    {
        private readonly ApplicationDbContext _context;

        public SeedDataController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> SeedMedicines()
        {
            try
            {
                // Kiểm tra xem đã có dữ liệu chưa
                if (_context.Medicines.Any())
                {
                    TempData["Info"] = "Dữ liệu thuốc đã tồn tại.";
                    return RedirectToAction("Index", "Medicine");
                }

                var medicines = new List<Medicine>
                {
                    new Medicine
                    {
                        Name = "Paracetamol 500mg",
                        Description = "Thuốc giảm đau, hạ sốt",
                        UnitPrice = 2000,
                        Unit = "Viên",
                        Manufacturer = "Pharma Co.",
                        IsActive = true,
                        Inventories = new List<Inventory>()
                    },
                    new Medicine
                    {
                        Name = "Amoxicillin 250mg",
                        Description = "Thuốc kháng sinh",
                        UnitPrice = 5000,
                        Unit = "Viên",
                        Manufacturer = "Antibiotic Ltd.",
                        IsActive = true,
                        Inventories = new List<Inventory>()
                    },
                    new Medicine
                    {
                        Name = "Vitamin C 1000mg",
                        Description = "Bổ sung vitamin C",
                        UnitPrice = 3000,
                        Unit = "Viên",
                        Manufacturer = "Vitamin Corp.",
                        IsActive = true,
                        Inventories = new List<Inventory>()
                    },
                    new Medicine
                    {
                        Name = "Cough Syrup",
                        Description = "Siro ho",
                        UnitPrice = 15000,
                        Unit = "Chai",
                        Manufacturer = "Cough Med Inc.",
                        IsActive = true,
                        Inventories = new List<Inventory>()
                    },
                    new Medicine
                    {
                        Name = "Aspirin 100mg",
                        Description = "Thuốc chống đông máu",
                        UnitPrice = 1500,
                        Unit = "Viên",
                        Manufacturer = "Heart Med Co.",
                        IsActive = true,
                        Inventories = new List<Inventory>()
                    }
                };

                await _context.Medicines.AddRangeAsync(medicines);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm {medicines.Count} thuốc mẫu thành công!";
                return RedirectToAction("Index", "Medicine");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm dữ liệu: " + ex.Message;
                return RedirectToAction("Index", "Medicine");
            }
        }

        public async Task<IActionResult> SeedInventory()
        {
            try
            {
                // Kiểm tra xem đã có thuốc chưa
                if (!_context.Medicines.Any())
                {
                    TempData["Error"] = "Cần thêm thuốc trước khi thêm tồn kho.";
                    return RedirectToAction("Index", "Medicine");
                }

                // Kiểm tra xem đã có nhân viên chưa
                if (!_context.Staffs.Any())
                {
                    TempData["Error"] = "Cần có nhân viên trong hệ thống trước khi thêm tồn kho.";
                    return RedirectToAction("Index", "Medicine");
                }

                var medicines = _context.Medicines.ToList();
                var firstStaff = _context.Staffs.First();

                var inventories = new List<Inventory>();

                foreach (var medicine in medicines.Take(3)) // Chỉ thêm tồn kho cho 3 thuốc đầu
                {
                    inventories.Add(new Inventory
                    {
                        MedicineId = medicine.MedicineId,
                        QuantityInStock = new Random().Next(50, 200),
                        ExpirationDate = DateTime.Now.AddMonths(new Random().Next(6, 24)),
                        ImportDate = DateTime.Now.AddDays(-new Random().Next(1, 30)),
                        ManagedByStaffId = firstStaff.StaffId,
                        Notes = "Dữ liệu mẫu",
                        Medicine = medicine,
                        ManagedByStaff = firstStaff
                    });
                }

                await _context.Inventories.AddRangeAsync(inventories);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm {inventories.Count} tồn kho mẫu thành công!";
                return RedirectToAction("Index", "Inventory");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm tồn kho: " + ex.Message;
                return RedirectToAction("Index", "Medicine");
            }
        }

        public async Task<IActionResult> ClearAll()
        {
            try
            {
                _context.Inventories.RemoveRange(_context.Inventories);
                _context.Medicines.RemoveRange(_context.Medicines);
                await _context.SaveChangesAsync();

                TempData["Success"] = "Đã xóa toàn bộ dữ liệu thuốc và tồn kho!";
                return RedirectToAction("Index", "Medicine");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi xóa dữ liệu: " + ex.Message;
                return RedirectToAction("Index", "Medicine");
            }
        }

        public async Task<IActionResult> SeedStaff()
        {
            try
            {
                // Kiểm tra xem đã có nhân viên chưa
                if (_context.Staffs.Any())
                {
                    TempData["Info"] = "Dữ liệu nhân viên đã tồn tại.";
                    return RedirectToAction("Index", "Medicine");
                }

                var staffs = new List<Staff>
                {
                    new Staff
                    {
                        FirstName = "Nguyễn",
                        LastName = "Văn A",
                        DateOfBirth = new DateTime(1990, 1, 1),
                        Gender = "Nam",
                        Address = "123 Đường ABC, TP.HCM",
                        Phone = "0123456789",
                        Email = "staff1@hospital.com",
                        Department = "Kho thuốc",
                        Position = "Nhân viên kho",
                        Salary = 8000000,
                        DateHired = DateTime.Now.AddYears(-2)
                    },
                    new Staff
                    {
                        FirstName = "Trần",
                        LastName = "Thị B",
                        DateOfBirth = new DateTime(1985, 5, 15),
                        Gender = "Nữ", 
                        Address = "456 Đường DEF, TP.HCM",
                        Phone = "0987654321",
                        Email = "staff2@hospital.com",
                        Department = "Dược",
                        Position = "Dược sĩ",
                        Salary = 12000000,
                        DateHired = DateTime.Now.AddYears(-3)
                    }
                };

                await _context.Staffs.AddRangeAsync(staffs);
                await _context.SaveChangesAsync();

                TempData["Success"] = $"Đã thêm {staffs.Count} nhân viên mẫu thành công!";
                return RedirectToAction("Index", "Medicine");
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra khi thêm nhân viên: " + ex.Message;
                return RedirectToAction("Index", "Medicine");
            }
        }
    }
}
