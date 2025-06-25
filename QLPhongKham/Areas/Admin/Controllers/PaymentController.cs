using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Models;
using QLPhongKham.Services;
using QLPhongKham.Data;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,NhanVien")]    public class PaymentController : Controller
    {
        private readonly IPaymentService _paymentService;
        private readonly IInvoiceService _invoiceService;
        private readonly IReportService _reportService;
        private readonly ApplicationDbContext _context;

        public PaymentController(IPaymentService paymentService, IInvoiceService invoiceService, IReportService reportService, ApplicationDbContext context)
        {
            _paymentService = paymentService;
            _invoiceService = invoiceService;
            _reportService = reportService;
            _context = context;
        }

        // GET: Admin/Payment
        public async Task<IActionResult> Index(string paymentMethod = "", DateTime? fromDate = null, DateTime? toDate = null)
        {
            ViewBag.CurrentPaymentMethod = paymentMethod;
            ViewBag.FromDate = fromDate?.ToString("yyyy-MM-dd");
            ViewBag.ToDate = toDate?.ToString("yyyy-MM-dd");

            IEnumerable<Payment> payments;

            if (!string.IsNullOrEmpty(paymentMethod))
            {
                payments = await _paymentService.GetPaymentsByMethodAsync(paymentMethod);
            }
            else if (fromDate.HasValue && toDate.HasValue)
            {
                payments = await _paymentService.GetPaymentsByDateRangeAsync(fromDate.Value, toDate.Value);
            }
            else
            {
                // Default to current month
                var startOfMonth = new DateTime(DateTime.Today.Year, DateTime.Today.Month, 1);
                var endOfMonth = startOfMonth.AddMonths(1).AddDays(-1);
                payments = await _paymentService.GetPaymentsByDateRangeAsync(startOfMonth, endOfMonth);
            }

            return View(payments.ToList());
        }

        // GET: Admin/Payment/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            return View(payment);
        }

        // GET: Admin/Payment/Create
        public async Task<IActionResult> Create(int? invoiceId)
        {
            if (invoiceId.HasValue)
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(invoiceId.Value);
                if (invoice != null)
                {
                    ViewBag.Invoice = invoice;
                    ViewBag.MaxAmount = invoice.RemainingAmount;
                }
            }

            var payment = new Payment
            {
                InvoiceId = invoiceId ?? 0,
                PaymentDate = DateTime.Now
            };

            return View(payment);
        }

        // POST: Admin/Payment/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Payment payment)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    var staffId = GetCurrentStaffId();
                    payment.CollectedByStaffId = staffId;
                    
                    await _paymentService.CreatePaymentAsync(payment);
                    TempData["SuccessMessage"] = "Thanh toán đã được ghi nhận thành công!";
                    return RedirectToAction("Details", "Invoice", new { id = payment.InvoiceId });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            // Reload invoice data for view
            if (payment.InvoiceId > 0)
            {
                var invoice = await _invoiceService.GetInvoiceByIdAsync(payment.InvoiceId);
                ViewBag.Invoice = invoice;
                ViewBag.MaxAmount = invoice?.RemainingAmount ?? 0;
            }

            return View(payment);
        }

        // POST: Admin/Payment/ProcessPayment
        [HttpPost]
        public async Task<IActionResult> ProcessPayment(int invoiceId, decimal amount, string paymentMethod, string notes)
        {
            try
            {
                var staffId = GetCurrentStaffId();
                var success = await _paymentService.ProcessPaymentAsync(invoiceId, amount, paymentMethod, staffId, notes);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Thanh toán đã được xử lý thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể xử lý thanh toán. Vui lòng kiểm tra lại thông tin!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Details", "Invoice", new { id = invoiceId });
        }

        // GET: Admin/Payment/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment == null)
            {
                return NotFound();
            }

            // Cannot edit completed payments older than 24 hours
            if (payment.Status == "Completed" && payment.PaymentDate < DateTime.Now.AddHours(-24))
            {
                TempData["ErrorMessage"] = "Không thể chỉnh sửa thanh toán đã được xác nhận quá 24 giờ!";
                return RedirectToAction("Details", new { id });
            }

            return View(payment);
        }

        // POST: Admin/Payment/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Payment payment)
        {
            if (id != payment.PaymentId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    await _paymentService.UpdatePaymentAsync(payment);
                    TempData["SuccessMessage"] = "Thanh toán đã được cập nhật thành công!";
                    return RedirectToAction("Details", new { id });
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
                }
            }

            return View(payment);
        }

        // POST: Admin/Payment/Refund
        [HttpPost]
        public async Task<IActionResult> Refund(int paymentId, decimal refundAmount, string reason)
        {
            try
            {
                var success = await _paymentService.RefundPaymentAsync(paymentId, refundAmount, reason);
                
                if (success)
                {
                    TempData["SuccessMessage"] = "Hoàn tiền đã được xử lý thành công!";
                }
                else
                {
                    TempData["ErrorMessage"] = "Không thể hoàn tiền. Vui lòng kiểm tra lại thông tin!";
                }
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Có lỗi xảy ra: " + ex.Message;
            }

            return RedirectToAction("Details", new { id = paymentId });
        }

        // GET: Admin/Payment/DailyReport
        public async Task<IActionResult> DailyReport(DateTime? date = null)
        {
            var reportDate = date ?? DateTime.Today;
            ViewBag.ReportDate = reportDate.ToString("yyyy-MM-dd");

            var payments = await _paymentService.GetPaymentsByDateRangeAsync(
                reportDate.Date, 
                reportDate.Date.AddDays(1).AddTicks(-1));

            var totalAmount = await _paymentService.GetTotalPaymentsByDateAsync(reportDate);
            ViewBag.TotalAmount = totalAmount;

            // Group by payment method
            var paymentsByMethod = payments
                .Where(p => p.Status == "Completed")
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new {
                    PaymentMethod = g.Key,
                    Count = g.Count(),
                    TotalAmount = g.Sum(p => p.AmountPaid)
                })
                .ToList();

            ViewBag.PaymentsByMethod = paymentsByMethod;

            return View(payments.ToList());
        }

        // GET: Admin/Payment/MonthlyReport
        public async Task<IActionResult> MonthlyReport(int? year = null, int? month = null)
        {
            var reportYear = year ?? DateTime.Today.Year;
            var reportMonth = month ?? DateTime.Today.Month;
            
            var startDate = new DateTime(reportYear, reportMonth, 1);
            var endDate = startDate.AddMonths(1).AddDays(-1);

            ViewBag.ReportYear = reportYear;
            ViewBag.ReportMonth = reportMonth;
            ViewBag.StartDate = startDate.ToString("yyyy-MM-dd");
            ViewBag.EndDate = endDate.ToString("yyyy-MM-dd");

            var payments = await _paymentService.GetPaymentsByDateRangeAsync(startDate, endDate);

            // Calculate totals by payment method
            var paymentMethods = new[] { "Cash", "Card", "Transfer", "Momo", "ZaloPay", "ViettelPay" };
            var methodTotals = new Dictionary<string, decimal>();

            foreach (var method in paymentMethods)
            {
                var total = await _paymentService.GetTotalPaymentsByMethodAsync(method, startDate, endDate);
                methodTotals[method] = total;
            }

            ViewBag.MethodTotals = methodTotals;
            ViewBag.GrandTotal = methodTotals.Values.Sum();

            return View(payments.ToList());
        }

        // GET: Admin/Payment/ExportReceiptPdf/5
        public async Task<IActionResult> ExportReceiptPdf(int id)
        {
            try
            {
                var pdfBytes = await _reportService.ExportPaymentReceiptToPdfAsync(id);
                var fileName = $"PhieuThu_{id}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(pdfBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất PDF: {ex.Message}";
                return RedirectToAction("Details", new { id });
            }
        }

        // GET: Admin/Payment/CreateForInvoice/5
        public async Task<IActionResult> CreateForInvoice(int invoiceId)
        {
            Console.WriteLine($"=== CreateForInvoice GET - InvoiceId: {invoiceId} ===");

            // Lấy thông tin invoice trực tiếp từ context
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Service)
                .Include(i => i.InvoiceDetails)
                    .ThenInclude(d => d.Medicine)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
            {
                TempData["Error"] = "Không tìm thấy hóa đơn.";
                return RedirectToAction("Index", "Invoice");
            }

            Console.WriteLine($"Invoice found: {invoice.InvoiceNumber}, Status: {invoice.Status}");
            Console.WriteLine($"Total Amount: {invoice.TotalAmount}, Paid Amount: {invoice.PaidAmount}");
            Console.WriteLine($"Remaining Amount: {invoice.RemainingAmount}");

            if (invoice.RemainingAmount <= 0)
            {
                TempData["Error"] = "Hóa đơn này đã được thanh toán đầy đủ.";
                return RedirectToAction("Details", "Invoice", new { id = invoiceId });
            }

            // Tạo model cho view
            var model = new Payment
            {
                InvoiceId = invoice.InvoiceId,
                PaymentDate = DateTime.Now,
                AmountPaid = invoice.RemainingAmount, // Mặc định thanh toán hết số tiền còn lại
                PaymentMethod = "Cash",
                Status = "Completed"
            };

            ViewBag.Invoice = invoice;
            ViewBag.PaymentMethods = new List<string> { "Cash", "Card", "Transfer", "Momo", "ZaloPay", "ViettelPay" };

            return View(model);
        }

        // POST: Admin/Payment/CreateForInvoice
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateForInvoice(Payment model)
        {
            Console.WriteLine("=== CreateForInvoice POST ===");
            Console.WriteLine($"InvoiceId: {model.InvoiceId}, Amount: {model.AmountPaid}");

            try
            {
                // Lấy invoice để validate
                var invoice = await _context.Invoices
                    .Include(i => i.Payments)
                    .FirstOrDefaultAsync(i => i.InvoiceId == model.InvoiceId);

                if (invoice == null)
                {
                    ModelState.AddModelError("", "Không tìm thấy hóa đơn.");
                    return View(model);
                }

                // Kiểm tra số tiền thanh toán
                if (model.AmountPaid <= 0)
                {
                    ModelState.AddModelError("AmountPaid", "Số tiền thanh toán phải lớn hơn 0.");
                    await ReloadCreateForInvoiceViewBagData(model.InvoiceId);
                    return View(model);
                }

                if (model.AmountPaid > invoice.RemainingAmount)
                {
                    ModelState.AddModelError("AmountPaid", $"Số tiền thanh toán không thể lớn hơn số tiền còn lại ({invoice.RemainingAmount:N0} VNĐ).");
                    await ReloadCreateForInvoiceViewBagData(model.InvoiceId);
                    return View(model);
                }

                // Tạo payment
                var payment = new Payment
                {
                    InvoiceId = model.InvoiceId,
                    PaymentNumber = await GeneratePaymentNumberAsync(),
                    PaymentDate = model.PaymentDate,
                    AmountPaid = model.AmountPaid,
                    PaymentMethod = model.PaymentMethod,
                    Status = model.Status,
                    BankTransactionId = model.BankTransactionId,
                    BankName = model.BankName,
                    CardLastFourDigits = model.CardLastFourDigits,
                    AuthorizationCode = model.AuthorizationCode,
                    TransactionFee = model.TransactionFee,
                    Notes = model.Notes,
                    CollectedByStaffId = GetCurrentStaffId(),
                    Invoice = invoice
                };

                _context.Payments.Add(payment);

                // Cập nhật số tiền đã thanh toán của invoice
                invoice.PaidAmount += model.AmountPaid;
                invoice.UpdatedDate = DateTime.Now;

                // Nếu thanh toán đủ, cập nhật trạng thái invoice
                if (invoice.RemainingAmount <= 0)
                {
                    invoice.Status = "Paid";
                }

                await _context.SaveChangesAsync();

                Console.WriteLine("Payment created successfully");
                TempData["Success"] = "Thanh toán thành công!";
                return RedirectToAction("Details", "Invoice", new { id = model.InvoiceId });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                ModelState.AddModelError("", "Có lỗi xảy ra: " + ex.Message);
                
                await ReloadCreateForInvoiceViewBagData(model.InvoiceId);
                return View(model);
            }
        }

        // Helper method cho CreateForInvoice
        private async Task<string> GeneratePaymentNumberAsync()
        {
            var year = DateTime.Now.Year;
            var lastPayment = await _context.Payments
                .Where(p => p.CreatedDate.Year == year)
                .OrderByDescending(p => p.PaymentId)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPayment != null)
            {
                var parts = lastPayment.PaymentNumber.Split('-');
                if (parts.Length == 3 && int.TryParse(parts[2], out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"PAY-{year}-{nextNumber:D4}";
        }

        private async Task ReloadCreateForInvoiceViewBagData(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            ViewBag.Invoice = invoice;
            ViewBag.PaymentMethods = new List<string> { "Cash", "Card", "Transfer", "Momo", "ZaloPay", "ViettelPay" };
        }

        private int GetCurrentStaffId()
        {
            // TODO: Implement logic to get current staff ID from logged-in user
            // This is a placeholder implementation
            return 1;
        }
    }
}
