using Microsoft.EntityFrameworkCore;
using QLPhongKham.Models;
using QLPhongKham.Data;

namespace QLPhongKham.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public PaymentService(ApplicationDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        public async Task<Payment> CreatePaymentAsync(Payment payment)
        {
            payment.PaymentNumber = await GeneratePaymentNumberAsync();
            payment.CreatedDate = DateTime.Now;
            payment.PaymentDate = DateTime.Now;

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            // Update invoice paid amount and status
            await UpdateInvoiceAfterPaymentAsync(payment.InvoiceId);

            return payment;
        }

        public async Task<Payment?> GetPaymentByIdAsync(int paymentId)
        {
            return await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
                .Include(p => p.CollectedByStaff)
                .FirstOrDefaultAsync(p => p.PaymentId == paymentId);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByInvoiceIdAsync(int invoiceId)
        {
            return await _context.Payments
                .Include(p => p.CollectedByStaff)
                .Where(p => p.InvoiceId == invoiceId)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
                .Include(p => p.CollectedByStaff)
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByMethodAsync(string paymentMethod)
        {
            return await _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
                .Where(p => p.PaymentMethod == paymentMethod)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Payment?> UpdatePaymentAsync(Payment payment)
        {
            payment.UpdatedDate = DateTime.Now;
            _context.Payments.Update(payment);
            await _context.SaveChangesAsync();

            // Update invoice after payment modification
            await UpdateInvoiceAfterPaymentAsync(payment.InvoiceId);

            return payment;
        }

        public async Task<bool> ProcessPaymentAsync(int invoiceId, decimal amount, string paymentMethod, int staffId, string? notes = null)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            // Validate payment amount
            if (amount <= 0 || amount > invoice.RemainingAmount) return false;

            var payment = new Payment
            {
                InvoiceId = invoiceId,
                AmountPaid = amount,
                PaymentMethod = paymentMethod,
                PaymentDate = DateTime.Now,
                Status = "Completed",
                CollectedByStaffId = staffId,
                Notes = notes
            };

            await CreatePaymentAsync(payment);
            return true;
        }

        public async Task<bool> RefundPaymentAsync(int paymentId, decimal refundAmount, string reason)
        {
            var originalPayment = await _context.Payments.FindAsync(paymentId);
            if (originalPayment == null) return false;

            // Validate refund amount
            if (refundAmount <= 0 || refundAmount > originalPayment.AmountPaid) return false;

            // Create refund payment (negative amount)
            var refundPayment = new Payment
            {
                InvoiceId = originalPayment.InvoiceId,
                AmountPaid = -refundAmount,
                PaymentMethod = originalPayment.PaymentMethod,
                PaymentDate = DateTime.Now,
                Status = "Refunded",
                CollectedByStaffId = originalPayment.CollectedByStaffId,
                Notes = $"Refund for payment {originalPayment.PaymentNumber}. Reason: {reason}"
            };

            await CreatePaymentAsync(refundPayment);

            // Update original payment status
            originalPayment.Status = "Refunded";
            originalPayment.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<string> GeneratePaymentNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"PAY{today:yyyyMM}";
            
            var lastPayment = await _context.Payments
                .Where(p => p.PaymentNumber.StartsWith(prefix))
                .OrderByDescending(p => p.PaymentNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastPayment != null)
            {
                var lastNumberStr = lastPayment.PaymentNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}{nextNumber:D4}";
        }

        public async Task<decimal> GetTotalPaymentsByDateAsync(DateTime date)
        {
            var startOfDay = date.Date;
            var endOfDay = startOfDay.AddDays(1).AddTicks(-1);

            return await _context.Payments
                .Where(p => p.PaymentDate >= startOfDay && p.PaymentDate <= endOfDay && p.Status == "Completed")
                .SumAsync(p => p.AmountPaid);
        }

        public async Task<decimal> GetTotalPaymentsByMethodAsync(string paymentMethod, DateTime fromDate, DateTime toDate)
        {
            return await _context.Payments
                .Where(p => p.PaymentMethod == paymentMethod && 
                           p.PaymentDate >= fromDate && 
                           p.PaymentDate <= toDate && 
                           p.Status == "Completed")
                .SumAsync(p => p.AmountPaid);
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStaffAsync(int staffId, DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Payments
                .Include(p => p.Invoice)
                .ThenInclude(i => i.Patient)
                .Where(p => p.CollectedByStaffId == staffId);

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            return await query
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        private async Task UpdateInvoiceAfterPaymentAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null) return;

            // Calculate total paid amount
            var totalPaid = invoice.Payments
                .Where(p => p.Status == "Completed")
                .Sum(p => p.AmountPaid);

            invoice.PaidAmount = totalPaid;
            // RemainingAmount is now a computed property, no need to set it

            // Update status based on payment
            if (invoice.RemainingAmount <= 0)
            {
                invoice.Status = "Paid";
            }
            else if (invoice.PaidAmount > 0)
            {
                invoice.Status = "Partial";
            }
            else if (invoice.DueDate.HasValue && invoice.DueDate < DateTime.Today)
            {
                invoice.Status = "Overdue";
            }
            else
            {
                invoice.Status = "Pending";
            }

            invoice.UpdatedDate = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
