using QLPhongKham.Models;

namespace QLPhongKham.Services
{
    public interface IReportService
    {
        // Excel Export Methods
        Task<byte[]> ExportInvoicesToExcelAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportPaymentsToExcelAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportOverdueInvoicesToExcelAsync();
        Task<byte[]> ExportFinancialSummaryToExcelAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportPatientInvoicesExcelAsync(int patientId);

        // PDF Export Methods
        Task<byte[]> ExportInvoicesToPdfAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportPaymentsToPdfAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportFinancialSummaryToPdfAsync(DateTime fromDate, DateTime toDate);
        Task<byte[]> ExportInvoiceDetailToPdfAsync(int invoiceId);
        Task<byte[]> ExportPaymentReceiptToPdfAsync(int paymentId);

        // Report Data Methods
        Task<object> GetFinancialSummaryAsync(DateTime fromDate, DateTime toDate);
        Task<object> GetRevenueByMethodAsync(DateTime fromDate, DateTime toDate);
        Task<object> GetTopPatientsAsync(DateTime fromDate, DateTime toDate, int topCount = 10);
        Task<object> GetServiceRevenueAsync(DateTime fromDate, DateTime toDate);
    }
}
