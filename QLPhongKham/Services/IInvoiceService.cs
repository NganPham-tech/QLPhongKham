using QLPhongKham.Models;

namespace QLPhongKham.Services
{    public interface IInvoiceService
    {
        Task<Invoice> CreateInvoiceFromAppointmentAsync(int appointmentId, int staffId);
        Task<Invoice> CreateInvoiceAsync(int patientId, int staffId, string invoiceType = "Service");
        Task<Invoice?> GetInvoiceByIdAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetInvoicesByPatientIdAsync(int patientId);
        Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(string status);
        Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync();        Task<Invoice?> UpdateInvoiceAsync(Invoice invoice);
        Task<bool> DeleteInvoiceAsync(int invoiceId);
        Task<string> GenerateInvoiceNumberAsync();
        Task<decimal> CalculateInvoiceTotalAsync(int invoiceId);
        Task<Invoice?> AddServiceToInvoiceAsync(int invoiceId, int serviceId, int quantity = 1);
        Task<Invoice?> AddMedicineToInvoiceAsync(int invoiceId, int medicineId, int quantity);
        Task<bool> MarkAsPaidAsync(int invoiceId);
        Task<bool> MarkAsOverdueAsync(int invoiceId);
        Task<IEnumerable<Invoice>> GetInvoicesForFinancialReportAsync(DateTime fromDate, DateTime toDate);
    }
}
