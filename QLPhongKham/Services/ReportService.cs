using QLPhongKham.Models;
using QLPhongKham.Data;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using OfficeOpenXml.Style;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using iText.Kernel.Font;
using iText.IO.Font.Constants;
using System.Drawing;
using Color = System.Drawing.Color;

namespace QLPhongKham.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
            // Set EPPlus license context
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        #region Excel Export Methods

        public async Task<byte[]> ExportInvoicesToExcelAsync(DateTime fromDate, DateTime toDate)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Include(i => i.Staff)
                .Where(i => i.CreatedDate >= fromDate && i.CreatedDate <= toDate)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Báo cáo hóa đơn");

            // Header
            worksheet.Cells["A1:K1"].Merge = true;
            worksheet.Cells["A1"].Value = $"BÁO CÁO HÓA ĐƠN TỪ {fromDate:dd/MM/yyyy} ĐẾN {toDate:dd/MM/yyyy}";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Column headers
            worksheet.Cells["A3"].Value = "Số HĐ";
            worksheet.Cells["B3"].Value = "Ngày tạo";
            worksheet.Cells["C3"].Value = "Bệnh nhân";
            worksheet.Cells["D3"].Value = "Loại HĐ";
            worksheet.Cells["E3"].Value = "Tổng tiền";
            worksheet.Cells["F3"].Value = "Đã thanh toán";
            worksheet.Cells["G3"].Value = "Còn nợ";
            worksheet.Cells["H3"].Value = "Trạng thái";
            worksheet.Cells["I3"].Value = "Ngày đáo hạn";
            worksheet.Cells["J3"].Value = "Nhân viên";
            worksheet.Cells["K3"].Value = "Ghi chú";

            // Style headers
            worksheet.Cells["A3:K3"].Style.Font.Bold = true;
            worksheet.Cells["A3:K3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A3:K3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            // Data
            int row = 4;
            foreach (var invoice in invoices)
            {
                worksheet.Cells[row, 1].Value = invoice.InvoiceNumber;
                worksheet.Cells[row, 2].Value = invoice.CreatedDate.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 3].Value = invoice.Patient?.FullName;
                worksheet.Cells[row, 4].Value = invoice.InvoiceType;
                worksheet.Cells[row, 5].Value = invoice.TotalAmount;
                worksheet.Cells[row, 6].Value = invoice.PaidAmount;
                worksheet.Cells[row, 7].Value = invoice.RemainingAmount;
                worksheet.Cells[row, 8].Value = invoice.Status;
                worksheet.Cells[row, 9].Value = invoice.DueDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 10].Value = invoice.Staff?.FullName;
                worksheet.Cells[row, 11].Value = invoice.Notes;

                // Format currency columns
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0";

                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Summary
            row += 2;
            worksheet.Cells[row, 1].Value = "TỔNG KẾT:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row + 1, 1].Value = "Tổng số hóa đơn:";
            worksheet.Cells[row + 1, 2].Value = invoices.Count;
            worksheet.Cells[row + 2, 1].Value = "Tổng doanh thu:";
            worksheet.Cells[row + 2, 2].Value = invoices.Sum(i => i.TotalAmount);
            worksheet.Cells[row + 2, 2].Style.Numberformat.Format = "#,##0";
            worksheet.Cells[row + 3, 1].Value = "Đã thu:";
            worksheet.Cells[row + 3, 2].Value = invoices.Sum(i => i.PaidAmount);
            worksheet.Cells[row + 3, 2].Style.Numberformat.Format = "#,##0";
            worksheet.Cells[row + 4, 1].Value = "Còn nợ:";
            worksheet.Cells[row + 4, 2].Value = invoices.Sum(i => i.RemainingAmount);
            worksheet.Cells[row + 4, 2].Style.Numberformat.Format = "#,##0";

            return package.GetAsByteArray();
        }        public async Task<byte[]> ExportPaymentsToExcelAsync(DateTime fromDate, DateTime toDate)
        {
            var payments = await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
                .Include(p => p.CollectedByStaff)
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Báo cáo thanh toán");

            // Header
            worksheet.Cells["A1:I1"].Merge = true;
            worksheet.Cells["A1"].Value = $"BÁO CÁO THANH TOÁN TỪ {fromDate:dd/MM/yyyy} ĐẾN {toDate:dd/MM/yyyy}";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Column headers
            worksheet.Cells["A3"].Value = "Số GD";
            worksheet.Cells["B3"].Value = "Ngày thanh toán";
            worksheet.Cells["C3"].Value = "Số HĐ";
            worksheet.Cells["D3"].Value = "Bệnh nhân";
            worksheet.Cells["E3"].Value = "Số tiền";
            worksheet.Cells["F3"].Value = "Phương thức";
            worksheet.Cells["G3"].Value = "Trạng thái";
            worksheet.Cells["H3"].Value = "Thu ngân";
            worksheet.Cells["I3"].Value = "Ghi chú";

            // Style headers
            worksheet.Cells["A3:I3"].Style.Font.Bold = true;
            worksheet.Cells["A3:I3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A3:I3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGreen);

            // Data
            int row = 4;
            foreach (var payment in payments)
            {
                worksheet.Cells[row, 1].Value = payment.PaymentNumber;
                worksheet.Cells[row, 2].Value = payment.PaymentDate.ToString("dd/MM/yyyy HH:mm");
                worksheet.Cells[row, 3].Value = payment.Invoice?.InvoiceNumber;
                worksheet.Cells[row, 4].Value = payment.Invoice?.Patient?.FullName;
                worksheet.Cells[row, 5].Value = payment.AmountPaid;
                worksheet.Cells[row, 6].Value = payment.PaymentMethod;
                worksheet.Cells[row, 7].Value = payment.Status;
                worksheet.Cells[row, 8].Value = payment.CollectedByStaff?.FullName;
                worksheet.Cells[row, 9].Value = payment.Notes;

                // Format currency column
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";

                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Summary by payment method
            row += 2;
            worksheet.Cells[row, 1].Value = "TỔNG KẾT THEO PHƯƠNG THỨC:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            var paymentSummary = payments.GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Total = g.Sum(p => p.AmountPaid), Count = g.Count() })
                .OrderByDescending(s => s.Total);

            foreach (var summary in paymentSummary)
            {
                worksheet.Cells[row, 1].Value = summary.Method;
                worksheet.Cells[row, 2].Value = summary.Count;
                worksheet.Cells[row, 3].Value = summary.Total;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                row++;
            }

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportOverdueInvoicesToExcelAsync()
        {
            var overdueInvoices = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Where(i => i.DueDate.HasValue && i.DueDate < DateTime.Now && i.RemainingAmount > 0)
                .OrderBy(i => i.DueDate)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Hóa đơn quá hạn");

            // Header
            worksheet.Cells["A1:J1"].Merge = true;
            worksheet.Cells["A1"].Value = $"DANH SÁCH HÓA ĐƠN QUÁ HẠN (Cập nhật: {DateTime.Now:dd/MM/yyyy HH:mm})";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Column headers
            worksheet.Cells["A3"].Value = "Số HĐ";
            worksheet.Cells["B3"].Value = "Bệnh nhân";
            worksheet.Cells["C3"].Value = "Điện thoại";
            worksheet.Cells["D3"].Value = "Ngày tạo";
            worksheet.Cells["E3"].Value = "Ngày đáo hạn";
            worksheet.Cells["F3"].Value = "Số ngày quá hạn";
            worksheet.Cells["G3"].Value = "Tổng tiền";
            worksheet.Cells["H3"].Value = "Đã thanh toán";
            worksheet.Cells["I3"].Value = "Còn nợ";
            worksheet.Cells["J3"].Value = "Ghi chú";

            // Style headers
            worksheet.Cells["A3:J3"].Style.Font.Bold = true;
            worksheet.Cells["A3:J3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A3:J3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightCoral);

            // Data
            int row = 4;
            foreach (var invoice in overdueInvoices)
            {
                var overdueDays = invoice.DueDate.HasValue ? (DateTime.Now - invoice.DueDate.Value).Days : 0;

                worksheet.Cells[row, 1].Value = invoice.InvoiceNumber;
                worksheet.Cells[row, 2].Value = invoice.Patient?.FullName;
                worksheet.Cells[row, 3].Value = invoice.Patient?.Phone;
                worksheet.Cells[row, 4].Value = invoice.CreatedDate.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 5].Value = invoice.DueDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 6].Value = overdueDays;
                worksheet.Cells[row, 7].Value = invoice.TotalAmount;
                worksheet.Cells[row, 8].Value = invoice.PaidAmount;
                worksheet.Cells[row, 9].Value = invoice.RemainingAmount;
                worksheet.Cells[row, 10].Value = invoice.Notes;

                // Format currency columns
                worksheet.Cells[row, 7].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 8].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 9].Style.Numberformat.Format = "#,##0";

                // Highlight very overdue invoices (>30 days)
                if (overdueDays > 30)
                {
                    worksheet.Cells[row, 1, row, 10].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[row, 1, row, 10].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.MistyRose);
                }

                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Summary
            row += 2;
            worksheet.Cells[row, 1].Value = "TỔNG KẾT:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row + 1, 1].Value = "Tổng số hóa đơn quá hạn:";
            worksheet.Cells[row + 1, 2].Value = overdueInvoices.Count;
            worksheet.Cells[row + 2, 1].Value = "Tổng nợ quá hạn:";
            worksheet.Cells[row + 2, 2].Value = overdueInvoices.Sum(i => i.RemainingAmount);
            worksheet.Cells[row + 2, 2].Style.Numberformat.Format = "#,##0";

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportFinancialSummaryToExcelAsync(DateTime fromDate, DateTime toDate)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Patient)
                .Where(i => i.CreatedDate >= fromDate && i.CreatedDate <= toDate)
                .ToListAsync();

            var payments = await _context.Payments
                .Include(p => p.Invoice)
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Tổng hợp tài chính");

            // Header
            worksheet.Cells["A1:F1"].Merge = true;
            worksheet.Cells["A1"].Value = $"BÁO CÁO TỔNG HỢP TÀI CHÍNH TỪ {fromDate:dd/MM/yyyy} ĐẾN {toDate:dd/MM/yyyy}";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            int row = 3;

            // Revenue summary
            worksheet.Cells[row, 1].Value = "1. TỔNG HỢP DOANH THU";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            worksheet.Cells[row, 1].Value = "Tổng hóa đơn tạo:";
            worksheet.Cells[row, 2].Value = invoices.Count;
            row++;

            worksheet.Cells[row, 1].Value = "Tổng giá trị hóa đơn:";
            worksheet.Cells[row, 2].Value = invoices.Sum(i => i.TotalAmount);
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
            row++;

            worksheet.Cells[row, 1].Value = "Tổng đã thu:";
            worksheet.Cells[row, 2].Value = payments.Sum(p => p.AmountPaid);
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
            row++;

            worksheet.Cells[row, 1].Value = "Tổng còn nợ:";
            worksheet.Cells[row, 2].Value = invoices.Sum(i => i.RemainingAmount);
            worksheet.Cells[row, 2].Style.Numberformat.Format = "#,##0";
            row += 2;

            // Payment method breakdown
            worksheet.Cells[row, 1].Value = "2. PHÂN TÍCH THEO PHƯƠNG THỨC THANH TOÁN";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            var paymentsByMethod = payments.GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count(), Total = g.Sum(p => p.AmountPaid) })
                .OrderByDescending(x => x.Total);

            worksheet.Cells[row, 1].Value = "Phương thức";
            worksheet.Cells[row, 2].Value = "Số lượng";
            worksheet.Cells[row, 3].Value = "Tổng tiền";
            worksheet.Cells[row, 4].Value = "Tỷ lệ %";
            worksheet.Cells[row, 1, row, 4].Style.Font.Bold = true;
            row++;

            var totalPayments = payments.Sum(p => p.AmountPaid);
            foreach (var method in paymentsByMethod)
            {
                worksheet.Cells[row, 1].Value = method.Method;
                worksheet.Cells[row, 2].Value = method.Count;
                worksheet.Cells[row, 3].Value = method.Total;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 4].Value = totalPayments > 0 ? (method.Total / totalPayments * 100) : 0;
                worksheet.Cells[row, 4].Style.Numberformat.Format = "0.0%";
                row++;
            }

            row += 2;

            // Daily breakdown
            worksheet.Cells[row, 1].Value = "3. PHÂN TÍCH THEO NGÀY";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            row++;

            var dailyStats = payments.GroupBy(p => p.PaymentDate.Date)
                .Select(g => new { Date = g.Key, Count = g.Count(), Total = g.Sum(p => p.AmountPaid) })
                .OrderBy(x => x.Date);

            worksheet.Cells[row, 1].Value = "Ngày";
            worksheet.Cells[row, 2].Value = "Số giao dịch";
            worksheet.Cells[row, 3].Value = "Tổng thu";
            worksheet.Cells[row, 1, row, 3].Style.Font.Bold = true;
            row++;

            foreach (var daily in dailyStats)
            {
                worksheet.Cells[row, 1].Value = daily.Date.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 2].Value = daily.Count;
                worksheet.Cells[row, 3].Value = daily.Total;
                worksheet.Cells[row, 3].Style.Numberformat.Format = "#,##0";
                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            return package.GetAsByteArray();
        }

        public async Task<byte[]> ExportPatientInvoicesExcelAsync(int patientId)
        {
            var patient = await _context.Patients.FindAsync(patientId);
            var invoices = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .ThenInclude(id => id.Service)
                .Include(i => i.InvoiceDetails)
                .ThenInclude(id => id.Medicine)
                .Include(i => i.Payments)
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("Lịch sử hóa đơn");

            // Header
            worksheet.Cells["A1:I1"].Merge = true;
            worksheet.Cells["A1"].Value = $"LỊCH SỬ HÓA ĐƠN BỆNH NHÂN: {patient?.FullName}";
            worksheet.Cells["A1"].Style.Font.Size = 16;
            worksheet.Cells["A1"].Style.Font.Bold = true;
            worksheet.Cells["A1"].Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;

            // Patient info
            worksheet.Cells["A2"].Value = $"Mã BN: {patient?.PatientId} | SĐT: {patient?.Phone} | Email: {patient?.Email}";
            worksheet.Cells["A2"].Style.Font.Italic = true;

            // Column headers
            worksheet.Cells["A4"].Value = "Số HĐ";
            worksheet.Cells["B4"].Value = "Ngày tạo";
            worksheet.Cells["C4"].Value = "Loại";
            worksheet.Cells["D4"].Value = "Tổng tiền";
            worksheet.Cells["E4"].Value = "Đã thanh toán";
            worksheet.Cells["F4"].Value = "Còn nợ";
            worksheet.Cells["G4"].Value = "Trạng thái";
            worksheet.Cells["H4"].Value = "Đáo hạn";
            worksheet.Cells["I4"].Value = "Ghi chú";

            // Style headers
            worksheet.Cells["A4:I4"].Style.Font.Bold = true;
            worksheet.Cells["A4:I4"].Style.Fill.PatternType = ExcelFillStyle.Solid;
            worksheet.Cells["A4:I4"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);

            // Data
            int row = 5;
            foreach (var invoice in invoices)
            {
                worksheet.Cells[row, 1].Value = invoice.InvoiceNumber;
                worksheet.Cells[row, 2].Value = invoice.CreatedDate.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 3].Value = invoice.InvoiceType;
                worksheet.Cells[row, 4].Value = invoice.TotalAmount;
                worksheet.Cells[row, 5].Value = invoice.PaidAmount;
                worksheet.Cells[row, 6].Value = invoice.RemainingAmount;
                worksheet.Cells[row, 7].Value = invoice.Status;
                worksheet.Cells[row, 8].Value = invoice.DueDate?.ToString("dd/MM/yyyy");
                worksheet.Cells[row, 9].Value = invoice.Notes;

                // Format currency columns
                worksheet.Cells[row, 4].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 5].Style.Numberformat.Format = "#,##0";
                worksheet.Cells[row, 6].Style.Numberformat.Format = "#,##0";

                row++;
            }

            // Auto-fit columns
            worksheet.Cells.AutoFitColumns();

            // Summary
            row += 2;
            worksheet.Cells[row, 1].Value = "TỔNG KẾT:";
            worksheet.Cells[row, 1].Style.Font.Bold = true;
            worksheet.Cells[row + 1, 1].Value = "Tổng hóa đơn:";
            worksheet.Cells[row + 1, 2].Value = invoices.Count;
            worksheet.Cells[row + 2, 1].Value = "Tổng giá trị:";
            worksheet.Cells[row + 2, 2].Value = invoices.Sum(i => i.TotalAmount);
            worksheet.Cells[row + 2, 2].Style.Numberformat.Format = "#,##0";
            worksheet.Cells[row + 3, 1].Value = "Tổng đã thanh toán:";
            worksheet.Cells[row + 3, 2].Value = invoices.Sum(i => i.PaidAmount);
            worksheet.Cells[row + 3, 2].Style.Numberformat.Format = "#,##0";
            worksheet.Cells[row + 4, 1].Value = "Tổng còn nợ:";
            worksheet.Cells[row + 4, 2].Value = invoices.Sum(i => i.RemainingAmount);
            worksheet.Cells[row + 4, 2].Style.Numberformat.Format = "#,##0";

            return package.GetAsByteArray();
        }

        #endregion

        #region PDF Export Methods

        public async Task<byte[]> ExportInvoicesToPdfAsync(DateTime fromDate, DateTime toDate)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Staff)
                .Where(i => i.CreatedDate >= fromDate && i.CreatedDate <= toDate)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Title
            document.Add(new Paragraph($"BÁO CÁO HÓA ĐƠN")
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Từ ngày {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(8);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            // Headers
            table.AddHeaderCell("Số HĐ");
            table.AddHeaderCell("Ngày tạo");
            table.AddHeaderCell("Bệnh nhân");
            table.AddHeaderCell("Loại HĐ");
            table.AddHeaderCell("Tổng tiền");
            table.AddHeaderCell("Đã TT");
            table.AddHeaderCell("Còn nợ");
            table.AddHeaderCell("Trạng thái");

            // Data
            foreach (var invoice in invoices)
            {
                table.AddCell(invoice.InvoiceNumber);
                table.AddCell(invoice.CreatedDate.ToString("dd/MM/yyyy"));
                table.AddCell(invoice.Patient?.FullName ?? "");
                table.AddCell(invoice.InvoiceType);
                table.AddCell(invoice.TotalAmount.ToString("#,##0"));
                table.AddCell(invoice.PaidAmount.ToString("#,##0"));
                table.AddCell(invoice.RemainingAmount.ToString("#,##0"));
                table.AddCell(invoice.Status);
            }

            document.Add(table);

            // Summary
            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph("TỔNG KẾT:").SetBold());
            document.Add(new Paragraph($"Tổng số hóa đơn: {invoices.Count}"));
            document.Add(new Paragraph($"Tổng doanh thu: {invoices.Sum(i => i.TotalAmount):#,##0} VND"));
            document.Add(new Paragraph($"Tổng đã thu: {invoices.Sum(i => i.PaidAmount):#,##0} VND"));
            document.Add(new Paragraph($"Tổng còn nợ: {invoices.Sum(i => i.RemainingAmount):#,##0} VND"));

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPaymentsToPdfAsync(DateTime fromDate, DateTime toDate)
        {            var payments = await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
                .Include(p => p.CollectedByStaff)
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Title
            document.Add(new Paragraph($"BÁO CÁO THANH TOÁN")
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Từ ngày {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            // Create table
            var table = new Table(7);
            table.SetWidth(UnitValue.CreatePercentValue(100));

            // Headers
            table.AddHeaderCell("Số GD");
            table.AddHeaderCell("Ngày TT");
            table.AddHeaderCell("Bệnh nhân");
            table.AddHeaderCell("Số tiền");
            table.AddHeaderCell("Phương thức");
            table.AddHeaderCell("Trạng thái");
            table.AddHeaderCell("Thu ngân");

            // Data
            foreach (var payment in payments)
            {
                table.AddCell(payment.PaymentNumber);
                table.AddCell(payment.PaymentDate.ToString("dd/MM/yyyy"));
                table.AddCell(payment.Invoice?.Patient?.FullName ?? "");
                table.AddCell(payment.AmountPaid.ToString("#,##0"));
                table.AddCell(payment.PaymentMethod);
                table.AddCell(payment.Status);
                table.AddCell(payment.CollectedByStaff?.FullName ?? "");
            }

            document.Add(table);

            // Summary
            document.Add(new Paragraph("\n"));
            document.Add(new Paragraph("TỔNG KẾT:").SetBold());
            document.Add(new Paragraph($"Tổng số giao dịch: {payments.Count}"));
            document.Add(new Paragraph($"Tổng tiền thu: {payments.Sum(p => p.AmountPaid):#,##0} VND"));

            // Summary by payment method
            var paymentSummary = payments.GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Total = g.Sum(p => p.AmountPaid), Count = g.Count() });

            document.Add(new Paragraph("\nPHÂN TÍCH THEO PHƯƠNG THỨC:").SetBold());
            foreach (var summary in paymentSummary)
            {
                document.Add(new Paragraph($"{summary.Method}: {summary.Count} giao dịch - {summary.Total:#,##0} VND"));
            }

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> ExportFinancialSummaryToPdfAsync(DateTime fromDate, DateTime toDate)
        {
            var invoices = await _context.Invoices
                .Where(i => i.CreatedDate >= fromDate && i.CreatedDate <= toDate)
                .ToListAsync();

            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .ToListAsync();

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Title
            document.Add(new Paragraph($"BÁO CÁO TỔNG HỢP TÀI CHÍNH")
                .SetFontSize(20)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Từ ngày {fromDate:dd/MM/yyyy} đến {toDate:dd/MM/yyyy}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Ngày xuất báo cáo: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            // Revenue summary
            document.Add(new Paragraph("1. TỔNG HỢP DOANH THU").SetBold().SetFontSize(14));
            document.Add(new Paragraph($"Tổng hóa đơn tạo: {invoices.Count} hóa đơn"));
            document.Add(new Paragraph($"Tổng giá trị hóa đơn: {invoices.Sum(i => i.TotalAmount):#,##0} VND"));
            document.Add(new Paragraph($"Tổng đã thu: {payments.Sum(p => p.AmountPaid):#,##0} VND"));
            document.Add(new Paragraph($"Tổng còn nợ: {invoices.Sum(i => i.RemainingAmount):#,##0} VND"));

            document.Add(new Paragraph("\n"));

            // Payment method breakdown
            document.Add(new Paragraph("2. PHÂN TÍCH THEO PHƯƠNG THỨC THANH TOÁN").SetBold().SetFontSize(14));
            var paymentsByMethod = payments.GroupBy(p => p.PaymentMethod)
                .Select(g => new { Method = g.Key, Count = g.Count(), Total = g.Sum(p => p.AmountPaid) })
                .OrderByDescending(x => x.Total);

            var totalPayments = payments.Sum(p => p.AmountPaid);
            foreach (var method in paymentsByMethod)
            {
                var percentage = totalPayments > 0 ? (method.Total / totalPayments * 100) : 0;
                document.Add(new Paragraph($"{method.Method}: {method.Count} giao dịch - {method.Total:#,##0} VND ({percentage:F1}%)"));
            }

            document.Add(new Paragraph("\n"));

            // Invoice status breakdown
            document.Add(new Paragraph("3. PHÂN TÍCH THEO TRẠNG THÁI HÓA ĐƠN").SetBold().SetFontSize(14));
            var invoicesByStatus = invoices.GroupBy(i => i.Status)
                .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(i => i.TotalAmount) });

            foreach (var status in invoicesByStatus)
            {
                document.Add(new Paragraph($"{status.Status}: {status.Count} hóa đơn - {status.Total:#,##0} VND"));
            }

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> ExportInvoiceDetailToPdfAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Staff)
                .Include(i => i.InvoiceDetails)
                .ThenInclude(id => id.Service)
                .Include(i => i.InvoiceDetails)
                .ThenInclude(id => id.Medicine)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null)
                throw new ArgumentException("Invoice not found");

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Header
            document.Add(new Paragraph("PHÒNG KHÁM ABC")
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Địa chỉ: 123 Đường ABC, Quận XYZ, TP.HCM")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("Điện thoại: 0123.456.789 | Email: info@phongkhamabc.com")
                .SetFontSize(10)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            // Invoice title
            document.Add(new Paragraph($"HÓA ĐƠN THANH TOÁN")
                .SetFontSize(16)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Số: {invoice.InvoiceNumber}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            // Patient info
            document.Add(new Paragraph("THÔNG TIN BỆNH NHÂN:").SetBold());
            document.Add(new Paragraph($"Họ tên: {invoice.Patient?.FullName}"));
            document.Add(new Paragraph($"Điện thoại: {invoice.Patient?.Phone}"));
            document.Add(new Paragraph($"Email: {invoice.Patient?.Email}"));
            document.Add(new Paragraph($"Địa chỉ: {invoice.Patient?.Address}"));

            document.Add(new Paragraph("\n"));

            // Invoice info
            document.Add(new Paragraph("THÔNG TIN HÓA ĐƠN:").SetBold());
            document.Add(new Paragraph($"Ngày tạo: {invoice.CreatedDate:dd/MM/yyyy HH:mm}"));
            document.Add(new Paragraph($"Loại hóa đơn: {invoice.InvoiceType}"));
            document.Add(new Paragraph($"Trạng thái: {invoice.Status}"));
            if (invoice.DueDate.HasValue)
                document.Add(new Paragraph($"Ngày đáo hạn: {invoice.DueDate:dd/MM/yyyy}"));

            document.Add(new Paragraph("\n"));

            // Invoice details
            document.Add(new Paragraph("CHI TIẾT HÓA ĐƠN:").SetBold());
            var detailTable = new Table(4);
            detailTable.SetWidth(UnitValue.CreatePercentValue(100));

            detailTable.AddHeaderCell("Tên dịch vụ/thuốc");
            detailTable.AddHeaderCell("Số lượng");
            detailTable.AddHeaderCell("Đơn giá");
            detailTable.AddHeaderCell("Thành tiền");

            foreach (var detail in invoice.InvoiceDetails)
            {
                var itemName = detail.Service?.Name ?? detail.Medicine?.Name ?? "N/A";
                detailTable.AddCell(itemName);
                detailTable.AddCell(detail.Quantity.ToString());
                detailTable.AddCell(detail.UnitPrice.ToString("#,##0"));
                detailTable.AddCell(detail.TotalPrice.ToString("#,##0"));
            }

            document.Add(detailTable);

            document.Add(new Paragraph("\n"));

            // Totals
            document.Add(new Paragraph($"Tổng tiền trước thuế: {invoice.SubTotal:#,##0} VND"));
            if (invoice.TaxAmount > 0)
                document.Add(new Paragraph($"Thuế VAT: {invoice.TaxAmount:#,##0} VND"));
            if (invoice.DiscountAmount > 0)
                document.Add(new Paragraph($"Giảm giá: {invoice.DiscountAmount:#,##0} VND"));
            document.Add(new Paragraph($"TỔNG CỘNG: {invoice.TotalAmount:#,##0} VND").SetBold());

            document.Add(new Paragraph("\n"));

            // Payment info
            if (invoice.Payments.Any())
            {
                document.Add(new Paragraph("THÔNG TIN THANH TOÁN:").SetBold());
                foreach (var payment in invoice.Payments)
                {
                    document.Add(new Paragraph($"- {payment.PaymentDate:dd/MM/yyyy}: {payment.AmountPaid:#,##0} VND ({payment.PaymentMethod})"));
                }
                document.Add(new Paragraph($"Tổng đã thanh toán: {invoice.PaidAmount:#,##0} VND"));
                document.Add(new Paragraph($"Còn lại: {invoice.RemainingAmount:#,##0} VND"));
            }

            document.Add(new Paragraph("\n"));

            // Footer
            document.Add(new Paragraph($"Nhân viên lập: {invoice.Staff?.FullName}")
                .SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Close();
            return stream.ToArray();
        }

        public async Task<byte[]> ExportPaymentReceiptToPdfAsync(int paymentId)
        {            var payment = await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
                .Include(p => p.CollectedByStaff)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);

            if (payment == null)
                throw new ArgumentException("Payment not found");

            using var stream = new MemoryStream();
            var writer = new PdfWriter(stream);
            var pdf = new PdfDocument(writer);
            var document = new Document(pdf);

            // Header
            document.Add(new Paragraph("PHÒNG KHÁM ABC")
                .SetFontSize(18)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("PHIẾU THU TIỀN")
                .SetFontSize(16)
                .SetBold()
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph($"Số: {payment.PaymentNumber}")
                .SetFontSize(12)
                .SetTextAlignment(TextAlignment.CENTER));

            document.Add(new Paragraph("\n"));

            // Payment info
            document.Add(new Paragraph($"Ngày thanh toán: {payment.PaymentDate:dd/MM/yyyy HH:mm}"));
            document.Add(new Paragraph($"Họ tên người nộp: {payment.Invoice?.Patient?.FullName}"));
            document.Add(new Paragraph($"Số hóa đơn: {payment.Invoice?.InvoiceNumber}"));
            document.Add(new Paragraph($"Số tiền: {payment.AmountPaid:#,##0} VND"));
            document.Add(new Paragraph($"Phương thức: {payment.PaymentMethod}"));
            document.Add(new Paragraph($"Trạng thái: {payment.Status}"));
            if (!string.IsNullOrEmpty(payment.Notes))
                document.Add(new Paragraph($"Ghi chú: {payment.Notes}"));

            document.Add(new Paragraph("\n"));

            // Footer
            document.Add(new Paragraph($"Thu ngân: {payment.CollectedByStaff?.FullName}")
                .SetTextAlignment(TextAlignment.RIGHT));
            document.Add(new Paragraph($"Ngày in: {DateTime.Now:dd/MM/yyyy HH:mm}")
                .SetTextAlignment(TextAlignment.RIGHT));

            document.Close();
            return stream.ToArray();
        }

        #endregion

        #region Report Data Methods

        public async Task<object> GetFinancialSummaryAsync(DateTime fromDate, DateTime toDate)
        {
            var invoices = await _context.Invoices
                .Where(i => i.CreatedDate >= fromDate && i.CreatedDate <= toDate)
                .ToListAsync();

            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .ToListAsync();

            return new
            {
                TotalInvoices = invoices.Count,
                TotalInvoiceAmount = invoices.Sum(i => i.TotalAmount),
                TotalPaidAmount = payments.Sum(p => p.AmountPaid),
                TotalRemainingAmount = invoices.Sum(i => i.RemainingAmount),
                InvoicesByStatus = invoices.GroupBy(i => i.Status)
                    .Select(g => new { Status = g.Key, Count = g.Count(), Total = g.Sum(i => i.TotalAmount) }),
                PaymentsByMethod = payments.GroupBy(p => p.PaymentMethod)
                    .Select(g => new { Method = g.Key, Count = g.Count(), Total = g.Sum(p => p.AmountPaid) })
            };
        }

        public async Task<object> GetRevenueByMethodAsync(DateTime fromDate, DateTime toDate)
        {
            var payments = await _context.Payments
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .GroupBy(p => p.PaymentMethod)
                .Select(g => new
                {
                    Method = g.Key,
                    Count = g.Count(),
                    Total = g.Sum(p => p.AmountPaid),
                    Percentage = 0.0 // Will be calculated after
                })
                .ToListAsync();

            var totalAmount = payments.Sum(p => p.Total);
            return payments.Select(p => new
            {
                p.Method,
                p.Count,
                p.Total,
                Percentage = totalAmount > 0 ? (double)p.Total / (double)totalAmount * 100 : 0
            });
        }

        public async Task<object> GetTopPatientsAsync(DateTime fromDate, DateTime toDate, int topCount = 10)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Where(i => i.CreatedDate >= fromDate && i.CreatedDate <= toDate)
                .GroupBy(i => new { i.PatientId, i.Patient.FullName, i.Patient.Phone })
                .Select(g => new
                {
                    PatientId = g.Key.PatientId,
                    PatientName = g.Key.FullName,
                    PhoneNumber = g.Key.Phone,
                    TotalInvoices = g.Count(),
                    TotalAmount = g.Sum(i => i.TotalAmount),
                    TotalPaid = g.Sum(i => i.PaidAmount),
                    TotalRemaining = g.Sum(i => i.RemainingAmount)
                })
                .OrderByDescending(x => x.TotalAmount)
                .Take(topCount)
                .ToListAsync();
        }        public async Task<object> GetServiceRevenueAsync(DateTime fromDate, DateTime toDate)
        {
            var invoiceDetails = await _context.InvoiceDetails
                .Include(id => id.Invoice)
                .Include(id => id.Service)
                .Include(id => id.Medicine)
                .Where(id => id.Invoice.CreatedDate >= fromDate && id.Invoice.CreatedDate <= toDate)
                .ToListAsync();

            return invoiceDetails
                .GroupBy(id => new
                {
                    Type = id.ServiceId.HasValue ? "Service" : "Medicine",
                    Name = id.ServiceId.HasValue ? 
                        (id.Service != null ? id.Service.Name : "Unknown Service") : 
                        (id.Medicine != null ? id.Medicine.Name : "Unknown Medicine")
                })
                .Select(g => new
                {
                    Type = g.Key.Type,
                    Name = g.Key.Name,
                    Quantity = g.Sum(id => id.Quantity),
                    Revenue = g.Sum(id => id.TotalPrice)
                })
                .OrderByDescending(x => x.Revenue)
                .ToList();
        }

        #endregion
    }
}
