using QLPhongKham.Models;

namespace QLPhongKham.Services
{
    public interface IPaymentService
    {
        Task<Payment> CreatePaymentAsync(Payment payment);
        Task<Payment?> GetPaymentByIdAsync(int paymentId);
        Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId);
        Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string paymentMethod);
        Task<Payment?> UpdatePaymentAsync(Payment payment);
        Task<bool> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod, int staffId, string? notes = null);
        Task<bool> RefundPaymentAsync(int paymentId, decimal refundAmount, string reason);
        Task<string> GeneratePaymentNumberAsync();
        Task<decimal> GetTotalPaymentsByDateAsync(DateTime date);
        Task<decimal> GetTotalPaymentsByMethodAsync(string paymentMethod, DateTime fromDate, DateTime toDate);
        Task<IEnumerable<Payment>> GetPaymentsByStaffAsync(int staffId, DateTime? fromDate = null, DateTime? toDate = null);
    }
}
