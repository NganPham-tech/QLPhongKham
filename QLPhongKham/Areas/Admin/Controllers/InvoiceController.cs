using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Models;
using QLPhongKham.Data;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NhanVien")]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;

        public InvoiceController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(string status = "", string search = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            ViewBag.CurrentStatus = status;
            ViewBag.CurrentSearch = search;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            
            var invoices = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Staff)
                .Include(i => i.Appointment)
                .ToListAsync();

            // Apply filters
            if (!string.IsNullOrEmpty(status))
            {
                invoices = invoices.Where(i => i.Status == status).ToList();
            }

            if (!string.IsNullOrEmpty(search))
            {
                invoices = invoices.Where(i => 
                    i.InvoiceNumber.Contains(search) ||
                    (i.Patient != null && i.Patient.FullName.Contains(search))).ToList();
            }

            if (fromDate.HasValue)
            {
                invoices = invoices.Where(i => i.CreatedDate >= fromDate.Value).ToList();
            }

            if (toDate.HasValue)
            {
                invoices = invoices.Where(i => i.CreatedDate <= toDate.Value).ToList();
            }

            return View(invoices);
        }

        // GET: Admin/Invoice/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Staff)
                .Include(i => i.Appointment)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Service)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Medicine)
                .FirstOrDefaultAsync(i => i.InvoiceId == id.Value);
                
            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // GET: Admin/Invoice/Create
        public async Task<IActionResult> Create()
        {
            // Lấy danh sách patients
            var patients = await _context.Patients.ToListAsync();
            ViewBag.Patients = patients;

            // Lấy danh sách services
            var services = await _context.Services.ToListAsync();
            ViewBag.Services = services;

            // Lấy danh sách appointments chưa có hóa đơn (tất cả trừ Cancelled)
            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .Where(a => a.Status != "Cancelled") // Cho phép tất cả trạng thái trừ Cancelled
                .Where(a => !_context.Invoices.Any(i => i.AppointmentId == a.AppointmentId)) // Chưa có hóa đơn
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
            ViewBag.Appointments = appointments;

            // Debug: Log số lượng appointments
            Console.WriteLine($"=== Create Invoice GET ===");
            Console.WriteLine($"Total appointments found: {appointments.Count}");
            foreach (var apt in appointments.Take(3))
            {
                Console.WriteLine($"- {apt.Patient?.FullName}: {apt.AppointmentDate:dd/MM/yyyy HH:mm} - {apt.Status}");
            }

            var invoice = new Invoice
            {
                InvoiceNumber = $"INV{DateTime.Now:yyyyMMddHHmmss}",
                CreatedDate = DateTime.Now,
                Status = "Pending",
                InvoiceType = "ServiceFee",
                IsElectronic = true,
                DiscountAmount = 0,
                TaxAmount = 0,
                PaidAmount = 0
            };

            return View(invoice);
        }

        // POST: Admin/Invoice/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Invoice model, int[] selectedServiceIds, decimal[] servicePrices)
        {
            try
            {
                if (ModelState.IsValid)
                {
                    // Tạo invoice number mới
                    model.InvoiceNumber = $"INV{DateTime.Now:yyyyMMddHHmmss}";
                    model.CreatedDate = DateTime.Now;
                    
                    // Tính toán các giá trị
                    decimal subTotal = 0;
                    
                    if (selectedServiceIds != null && servicePrices != null)
                    {
                        for (int i = 0; i < selectedServiceIds.Length; i++)
                        {
                            subTotal += servicePrices[i];
                        }
                    }

                    model.SubTotal = subTotal;
                    model.TotalAmount = subTotal + model.TaxAmount - model.DiscountAmount;

                    // Lưu invoice
                    _context.Invoices.Add(model);
                    await _context.SaveChangesAsync();

                    // Tạo invoice details nếu có services
                    if (selectedServiceIds != null && servicePrices != null)
                    {
                        for (int i = 0; i < selectedServiceIds.Length; i++)
                        {
                            var service = await _context.Services.FindAsync(selectedServiceIds[i]);
                            if (service != null)
                            {
                                var detail = new InvoiceDetail
                                {
                                    Invoice = model, // Set required navigation property
                                    ServiceId = selectedServiceIds[i],
                                    Quantity = 1,
                                    UnitPrice = servicePrices[i],
                                    Notes = service.Name
                                };
                                _context.InvoiceDetails.Add(detail);
                            }
                        }
                        await _context.SaveChangesAsync();
                    }

                    TempData["SuccessMessage"] = "Tạo hóa đơn thành công!";
                    return RedirectToAction(nameof(Index));
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi khi tạo hóa đơn: {ex.Message}";
            }

            // Reload data if validation fails
            var patients = await _context.Patients.ToListAsync();
            ViewBag.Patients = patients;

            var services = await _context.Services.ToListAsync();
            ViewBag.Services = services;

            var appointments = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .Where(a => a.Status == "Completed" || a.Status == "Confirmed")
                .ToListAsync();
            ViewBag.Appointments = appointments;

            return View(model);
        }

        // GET: Admin/Invoice/CreateFromAppointment/5
        public async Task<IActionResult> CreateFromAppointment(int appointmentId)
        {
            Console.WriteLine($"=== CreateFromAppointment GET - AppointmentId: {appointmentId} ===");

            // Lấy thông tin appointment
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            Console.WriteLine($"Appointment found: {appointment != null}");

            if (appointment == null)
            {
                Console.WriteLine("Appointment not found");
                TempData["Error"] = "Không tìm thấy cuộc hẹn.";
                return RedirectToAction("Index", "Appointment");
            }

            Console.WriteLine($"Appointment status: {appointment.Status}");

            // Chỉ từ chối cuộc hẹn đã bị hủy
            if (appointment.Status == "Cancelled")
            {
                Console.WriteLine("Cannot create invoice for cancelled appointment");
                TempData["Error"] = "Không thể tạo hóa đơn cho cuộc hẹn đã hủy.";
                return RedirectToAction("Index", "Appointment");
            }

            // Kiểm tra đã có hóa đơn chưa
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

            Console.WriteLine($"Existing invoice found: {existingInvoice != null}");

            if (existingInvoice != null)
            {
                Console.WriteLine("Invoice already exists");
                TempData["Error"] = "Cuộc hẹn này đã có hóa đơn.";
                return RedirectToAction("Details", new { id = existingInvoice.InvoiceId });
            }

            // Tạo model cho view
            var model = new Invoice
            {
                PatientId = appointment.PatientId,
                AppointmentId = appointment.AppointmentId,
                CreatedDate = DateTime.Now,
                Status = "Pending"
            };

            // Load danh sách medicines cho form
            ViewBag.Medicines = await _context.Medicines
                .Where(m => m.IsActive)
                .Select(m => new { m.MedicineId, m.Name, Price = m.UnitPrice })
                .ToListAsync();

            // Load danh sách services cho form  
            ViewBag.Services = await _context.Services
                .Where(s => s.IsActive)
                .Select(s => new { s.ServiceId, s.Name, s.Price })
                .ToListAsync();

            ViewBag.Appointment = appointment;

            Console.WriteLine("Returning CreateFromAppointment view");
            return View(model);
        }

        // POST: Admin/Invoice/CreateFromAppointment
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateFromAppointment(Invoice model, 
            int[] additionalServiceIds, int[] additionalServiceQuantities,
            int[] medicineIds, int[] medicineQuantities)
        {
            Console.WriteLine("=== CreateFromAppointment POST ===");
            Console.WriteLine($"AppointmentId: {model.AppointmentId}");

            try
            {
                // Lấy appointment để validate
                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Service)
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);

                if (appointment == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy cuộc hẹn.");
                    return View(model);
                }

                // Tạo invoice
                var invoice = new Invoice
                {
                    PatientId = appointment.PatientId,
                    AppointmentId = appointment.AppointmentId,
                    InvoiceNumber = await GenerateInvoiceNumberAsync(),
                    CreatedDate = DateTime.Now,
                    Status = "Pending",
                    Notes = model.Notes,
                    StaffId = GetCurrentStaffId() // Helper method để lấy staff hiện tại
                };

                _context.Invoices.Add(invoice);
                await _context.SaveChangesAsync();

                // Thêm dịch vụ chính từ appointment
                var mainServiceDetail = new InvoiceDetail
                {
                    InvoiceId = invoice.InvoiceId,
                    ServiceId = appointment.ServiceId,
                    Quantity = 1,
                    UnitPrice = appointment.Service!.Price,
                    Invoice = invoice
                };
                _context.InvoiceDetails.Add(mainServiceDetail);

                decimal totalAmount = appointment.Service.Price;

                // Thêm dịch vụ phát sinh (nếu có)
                if (additionalServiceIds != null && additionalServiceIds.Length > 0)
                {
                    for (int i = 0; i < additionalServiceIds.Length; i++)
                    {
                        if (additionalServiceIds[i] > 0 && additionalServiceQuantities[i] > 0)
                        {
                            var service = await _context.Services.FindAsync(additionalServiceIds[i]);
                            if (service != null)
                            {
                                var serviceDetail = new InvoiceDetail
                                {
                                    InvoiceId = invoice.InvoiceId,
                                    ServiceId = service.ServiceId,
                                    Quantity = additionalServiceQuantities[i],
                                    UnitPrice = service.Price,
                                    Invoice = invoice
                                };
                                _context.InvoiceDetails.Add(serviceDetail);
                                totalAmount += serviceDetail.TotalPrice;
                            }
                        }
                    }
                }

                // Thêm thuốc (nếu có)
                if (medicineIds != null && medicineIds.Length > 0)
                {
                    for (int i = 0; i < medicineIds.Length; i++)
                    {
                        if (medicineIds[i] > 0 && medicineQuantities[i] > 0)
                        {
                            var medicine = await _context.Medicines.FindAsync(medicineIds[i]);
                            if (medicine != null)
                            {
                                var medicineDetail = new InvoiceDetail
                                {
                                    InvoiceId = invoice.InvoiceId,
                                    MedicineId = medicine.MedicineId,
                                    Quantity = medicineQuantities[i],
                                    UnitPrice = medicine.UnitPrice,
                                    Invoice = invoice
                                };
                                _context.InvoiceDetails.Add(medicineDetail);
                                totalAmount += medicineDetail.TotalPrice;
                            }
                        }
                    }
                }

                // Cập nhật tổng tiền
                invoice.TotalAmount = totalAmount;
                invoice.PaidAmount = 0;
                // RemainingAmount sẽ được tính tự động trong model

                await _context.SaveChangesAsync();

                Console.WriteLine("Invoice created successfully");
                TempData["Success"] = "Tạo hóa đơn thành công!";
                return RedirectToAction("Details", new { id = invoice.InvoiceId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                
                // Reload ViewBag data
                ViewBag.Medicines = await _context.Medicines
                    .Where(m => m.IsActive)
                    .Select(m => new { m.MedicineId, m.Name, Price = m.UnitPrice })
                    .ToListAsync();

                ViewBag.Services = await _context.Services
                    .Where(s => s.IsActive)
                    .Select(s => new { s.ServiceId, s.Name, s.Price })
                    .ToListAsync();

                var appointment = await _context.Appointments
                    .Include(a => a.Patient)
                    .Include(a => a.Service)
                    .Include(a => a.Doctor)
                    .FirstOrDefaultAsync(a => a.AppointmentId == model.AppointmentId);
                ViewBag.Appointment = appointment;

                return View(model);
            }
        }

        // Helper methods
        private async Task<string> GenerateInvoiceNumberAsync()
        {
            var year = DateTime.Now.Year;
            var lastInvoice = await _context.Invoices
                .Where(i => i.CreatedDate.Year == year)
                .OrderByDescending(i => i.InvoiceId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                // Extract number from last invoice number (format: INV-2025-001)
                var parts = lastInvoice.InvoiceNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"INV-{year}-{nextNumber:D3}";
        }

        private int? GetCurrentStaffId()
        {
            // TODO: Implement logic to get current staff ID from user claims
            // For now, return null or a default staff ID
            return null;
        }

        // GET: Admin/Invoice/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Staff)
                .Include(i => i.Appointment)
                .Include(i => i.InvoiceDetails)
                .FirstOrDefaultAsync(i => i.InvoiceId == id.Value);
                
            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // POST: Admin/Invoice/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Invoice invoice)
        {
            if (id != invoice.InvoiceId) return NotFound();

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(invoice);
                    await _context.SaveChangesAsync();
                    TempData["Success"] = "Cập nhật hóa đơn thành công!";
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                }
            }

            return View(invoice);
        }

        // GET: Admin/Invoice/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null) return NotFound();

            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Staff)
                .Include(i => i.Appointment)
                .Include(i => i.InvoiceDetails)
                .FirstOrDefaultAsync(i => i.InvoiceId == id.Value);
                
            if (invoice == null) return NotFound();

            return View(invoice);
        }

        // POST: Admin/Invoice/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                var invoice = await _context.Invoices
                    .Include(i => i.InvoiceDetails)
                    .FirstOrDefaultAsync(i => i.InvoiceId == id);
                    
                if (invoice != null)
                {
                    _context.InvoiceDetails.RemoveRange(invoice.InvoiceDetails);
                    _context.Invoices.Remove(invoice);
                    await _context.SaveChangesAsync();
                }
                
                TempData["Success"] = "Xóa hóa đơn thành công!";
            }
            catch (Exception ex)
            {
                TempData["Error"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction(nameof(Index));
        }
    }
}