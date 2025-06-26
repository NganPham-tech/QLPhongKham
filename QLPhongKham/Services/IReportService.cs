using QLPhongKham.Models;

namespace QLPhongKham.Services
{
    public interface IReportService
    {
        Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetTotalAppointmentsAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<int> GetTotalPatientsAsync();
        Task<decimal> GetTotalPendingPaymentsAsync();
        Task<List<Invoice>> GetOverdueInvoicesAsync();
        Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<Dictionary<string, decimal>> GetRevenueByServiceAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<Dictionary<string, int>> GetAppointmentsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null);
        
       
        Task<object> GetFinancialSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportInvoicesToExcelAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportPaymentsToExcelAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportOverdueInvoicesToExcelAsync();
        Task<byte[]> ExportFinancialSummaryToExcelAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportPatientInvoicesExcelAsync(int patientId);
        Task<byte[]> ExportInvoicesToPdfAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportPaymentsToPdfAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportFinancialSummaryToPdfAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<byte[]> ExportInvoiceDetailToPdfAsync(int invoiceId);
        Task<byte[]> ExportPaymentReceiptToPdfAsync(int paymentId);
        Task<Dictionary<string, decimal>> GetRevenueByMethodAsync(DateTime? fromDate = null, DateTime? toDate = null);
        Task<List<object>> GetTopPatientsAsync(int count = 10);
        Task<List<object>> GetServiceRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null);
    }
}
