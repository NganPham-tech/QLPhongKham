using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QLPhongKham.Services;
using System.Globalization;

namespace QLPhongKham.Areas.Admin.Controllers
{
    [Authorize(Roles = "Admin,NhanVien")]
    [Area("Admin")]
    public class ReportController : Controller
    {
        private readonly IReportService _reportService;
        private readonly IInvoiceService _invoiceService;
        private readonly IPaymentService _paymentService;

        public ReportController(IReportService reportService, IInvoiceService invoiceService, IPaymentService paymentService)
        {
            _reportService = reportService;
            _invoiceService = invoiceService;
            _paymentService = paymentService;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public IActionResult FinancialReport()
        {
            var model = new
            {
                FromDate = DateTime.Now.AddMonths(-1).ToString("yyyy-MM-dd"),
                ToDate = DateTime.Now.ToString("yyyy-MM-dd")
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> FinancialReportData(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var data = await _reportService.GetFinancialSummaryAsync(fromDate, toDate);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // Excel Exports
        [HttpPost]
        public async Task<IActionResult> ExportInvoicesExcel(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var fileBytes = await _reportService.ExportInvoicesToExcelAsync(fromDate, toDate);
                var fileName = $"BaoCaoHoaDon_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportPaymentsExcel(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var fileBytes = await _reportService.ExportPaymentsToExcelAsync(fromDate, toDate);
                var fileName = $"BaoCaoThanhToan_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportOverdueInvoicesExcel()
        {
            try
            {
                var fileBytes = await _reportService.ExportOverdueInvoicesToExcelAsync();
                var fileName = $"DanhSachHoaDonQuaHan_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportFinancialSummaryExcel(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var fileBytes = await _reportService.ExportFinancialSummaryToExcelAsync(fromDate, toDate);
                var fileName = $"TongHopTaiChinh_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportPatientInvoicesExcel(int patientId)
        {
            try
            {
                var fileBytes = await _reportService.ExportPatientInvoicesExcelAsync(patientId);
                var fileName = $"LichSuHoaDon_BenhNhan_{patientId}_{DateTime.Now:yyyyMMdd}.xlsx";
                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        // PDF Exports
        [HttpPost]
        public async Task<IActionResult> ExportInvoicesPdf(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var fileBytes = await _reportService.ExportInvoicesToPdfAsync(fromDate, toDate);
                var fileName = $"BaoCaoHoaDon_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportPaymentsPdf(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var fileBytes = await _reportService.ExportPaymentsToPdfAsync(fromDate, toDate);
                var fileName = $"BaoCaoThanhToan_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportFinancialSummaryPdf(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var fileBytes = await _reportService.ExportFinancialSummaryToPdfAsync(fromDate, toDate);
                var fileName = $"TongHopTaiChinh_{fromDate:yyyyMMdd}_{toDate:yyyyMMdd}.pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportInvoiceDetailPdf(int invoiceId)
        {
            try
            {
                var fileBytes = await _reportService.ExportInvoiceDetailToPdfAsync(invoiceId);
                var fileName = $"HoaDon_{invoiceId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

        [HttpPost]
        public async Task<IActionResult> ExportPaymentReceiptPdf(int paymentId)
        {
            try
            {
                var fileBytes = await _reportService.ExportPaymentReceiptToPdfAsync(paymentId);
                var fileName = $"PhieuThu_{paymentId}_{DateTime.Now:yyyyMMdd}.pdf";
                return File(fileBytes, "application/pdf", fileName);
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Lỗi xuất báo cáo: {ex.Message}";
                return RedirectToAction("Index");
            }
        }

       
        [HttpGet]
        public async Task<IActionResult> GetRevenueByMethod(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var data = await _reportService.GetRevenueByMethodAsync(fromDate, toDate);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetTopPatients(DateTime fromDate, DateTime toDate, int topCount = 10)
        {
            try
            {
                var data = await _reportService.GetTopPatientsAsync(topCount);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetServiceRevenue(DateTime fromDate, DateTime toDate)
        {
            try
            {
                var data = await _reportService.GetServiceRevenueAsync(fromDate, toDate);
                return Json(new { success = true, data });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
