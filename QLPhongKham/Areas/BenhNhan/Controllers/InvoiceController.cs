using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;
using QLPhongKham.Services;

namespace QLPhongKham.Areas.BenhNhan.Controllers
{
    [Area("BenhNhan")]
    [Authorize(Roles = "Patient")]
    public class InvoiceController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;
        private readonly IPatientLinkingService _patientLinkingService;

        public InvoiceController(
            ApplicationDbContext context,
            IInvoiceService invoiceService,
            IPatientLinkingService patientLinkingService)
        {
            _context = context;
            _invoiceService = invoiceService;
            _patientLinkingService = patientLinkingService;        }

        // Danh sách hóa đơn của bệnh nhân
        public async Task<IActionResult> Index()
        {
            try
            {
                // Lấy thông tin bệnh nhân từ user hiện tại
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Không thể xác định người dùng.";
                    return RedirectToAction("Index", "Home");
                }

                var patient = await _patientLinkingService.GetPatientByUserIdAsync(userId);
                if (patient == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin bệnh nhân.";
                    return RedirectToAction("Index", "Home");
                }

                // Lấy danh sách hóa đơn của bệnh nhân
                var invoices = await _context.Invoices
                    .Where(i => i.PatientId == patient.PatientId)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Service)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Medicine)
                    .Include(i => i.Payments)
                    .OrderByDescending(i => i.CreatedDate)
                    .ToListAsync();

                return View(invoices);            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách hóa đơn.";
                return RedirectToAction("Index", "Home");
            }
        }        // Chi tiết hóa đơn
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                // Lấy thông tin bệnh nhân từ user hiện tại
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "Không thể xác định người dùng.";
                    return RedirectToAction("Index");
                }

                var patient = await _patientLinkingService.GetPatientByUserIdAsync(userId);
                if (patient == null)
                {
                    TempData["Error"] = "Không tìm thấy thông tin bệnh nhân.";
                    return RedirectToAction("Index");
                }

                // Lấy hóa đơn với điều kiện thuộc về bệnh nhân hiện tại
                var invoice = await _context.Invoices
                    .Where(i => i.InvoiceId == id && i.PatientId == patient.PatientId)
                    .Include(i => i.Patient)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Service)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Medicine)
                    .Include(i => i.Payments)
                    .FirstOrDefaultAsync();

                if (invoice == null)
                {
                    TempData["Error"] = "Không tìm thấy hóa đơn hoặc bạn không có quyền xem hóa đơn này.";
                    return RedirectToAction("Index");
                }

                return View(invoice);            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi tải chi tiết hóa đơn.";
                return RedirectToAction("Index");
            }
        }        // In hóa đơn
        public async Task<IActionResult> Print(int id)
        {
            try
            {
                // Lấy thông tin bệnh nhân từ user hiện tại
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return NotFound();
                }

                var patient = await _patientLinkingService.GetPatientByUserIdAsync(userId);
                if (patient == null)
                {
                    return NotFound();
                }

                // Lấy hóa đơn với điều kiện thuộc về bệnh nhân hiện tại
                var invoice = await _context.Invoices
                    .Where(i => i.InvoiceId == id && i.PatientId == patient.PatientId)
                    .Include(i => i.Patient)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Service)
                    .Include(i => i.InvoiceDetails)
                        .ThenInclude(d => d.Medicine)
                    .Include(i => i.Payments)
                    .FirstOrDefaultAsync();

                if (invoice == null)
                {
                    return NotFound();
                }

                return View("PrintInvoice", invoice);            }
            catch (Exception)
            {
                TempData["Error"] = "Có lỗi xảy ra khi in hóa đơn.";
                return RedirectToAction("Index");
            }
        }

        // Tải xuống hóa đơn PDF (có thể implement sau)
        public Task<IActionResult> DownloadPDF(int id)
        {
            // TODO: Implement PDF generation
            TempData["Info"] = "Chức năng tải xuống PDF sẽ được triển khai sau.";
            return Task.FromResult<IActionResult>(RedirectToAction("Details", new { id }));
        }
    }
}
