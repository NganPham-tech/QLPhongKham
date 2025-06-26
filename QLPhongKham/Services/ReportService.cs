using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Services
{
    public class ReportService : IReportService
    {
        private readonly ApplicationDbContext _context;

        public ReportService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<decimal> GetTotalRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Payments.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            return await query.SumAsync(p => p.AmountPaid);
        }

        public async Task<int> GetTotalAppointmentsAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);

            return await query.CountAsync();
        }

        public async Task<int> GetTotalPatientsAsync()
        {
            return await _context.Patients.CountAsync();
        }

        public async Task<decimal> GetTotalPendingPaymentsAsync()
        {
            return await _context.Invoices
                .Where(i => i.Status == "Pending")
                .SumAsync(i => i.TotalAmount - i.PaidAmount);
        }

        public async Task<List<Invoice>> GetOverdueInvoicesAsync()
        {
            var overdueDate = DateTime.Now.AddDays(-30); // 30 days overdue
            return await _context.Invoices
                .Include(i => i.Patient)
                .Where(i => i.Status == "Pending" && i.CreatedDate < overdueDate)
                .ToListAsync();
        }

        public async Task<List<Payment>> GetPaymentsByDateRangeAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Payments
                .Include(p => p.Invoice)
                    .ThenInclude(i => i.Patient)
                .Where(p => p.PaymentDate >= fromDate && p.PaymentDate <= toDate)
                .OrderByDescending(p => p.PaymentDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByServiceAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Invoice)
                    .ThenInclude(i => i.Payments)
                .Where(a => a.Status == "Completed");

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);

            var appointments = await query.ToListAsync();

            return appointments
                .GroupBy(a => a.Service.Name)
                .ToDictionary(
                    g => g.Key,
                    g => g.Sum(a => a.Invoice?.Payments?.Sum(p => p.AmountPaid) ?? 0)
                );
        }

        public async Task<Dictionary<string, int>> GetAppointmentsByStatusAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Appointments.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);

            var appointments = await query.ToListAsync();

            return appointments
                .GroupBy(a => a.Status)
                .ToDictionary(g => g.Key, g => g.Count());
        }

        // Additional methods for reports and exports
        public async Task<object> GetFinancialSummaryAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            return new
            {
                TotalRevenue = await GetTotalRevenueAsync(fromDate, toDate),
                TotalAppointments = await GetTotalAppointmentsAsync(fromDate, toDate),
                PendingPayments = await GetTotalPendingPaymentsAsync(),
                RevenueByService = await GetRevenueByServiceAsync(fromDate, toDate)
            };
        }

        public async Task<byte[]> ExportInvoicesToExcelAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Placeholder - implement Excel export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportPaymentsToExcelAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Placeholder - implement Excel export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportOverdueInvoicesToExcelAsync()
        {
            // Placeholder - implement Excel export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportFinancialSummaryToExcelAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Placeholder - implement Excel export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportPatientInvoicesExcelAsync(int patientId)
        {
            // Placeholder - implement Excel export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportInvoicesToPdfAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Placeholder - implement PDF export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportPaymentsToPdfAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Placeholder - implement PDF export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportFinancialSummaryToPdfAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            // Placeholder - implement PDF export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportInvoiceDetailToPdfAsync(int invoiceId)
        {
            // Placeholder - implement PDF export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<byte[]> ExportPaymentReceiptToPdfAsync(int paymentId)
        {
            // Placeholder - implement PDF export
            await Task.CompletedTask;
            return new byte[0];
        }

        public async Task<Dictionary<string, decimal>> GetRevenueByMethodAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Payments.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(p => p.PaymentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(p => p.PaymentDate <= toDate.Value);

            var payments = await query.ToListAsync();

            return payments
                .GroupBy(p => p.PaymentMethod)
                .ToDictionary(g => g.Key, g => g.Sum(p => p.AmountPaid));
        }

        public async Task<List<object>> GetTopPatientsAsync(int count = 10)
        {
            var topPatients = await _context.Invoices
                .Include(i => i.Patient)
                .GroupBy(i => i.Patient)
                .Select(g => new 
                {
                    Patient = g.Key,
                    TotalSpent = g.Sum(i => i.TotalAmount),
                    VisitCount = g.Count()
                })
                .OrderByDescending(x => x.TotalSpent)
                .Take(count)
                .ToListAsync();

            return topPatients.Cast<object>().ToList();
        }

        public async Task<List<object>> GetServiceRevenueAsync(DateTime? fromDate = null, DateTime? toDate = null)
        {
            var query = _context.Appointments
                .Include(a => a.Service)
                .Include(a => a.Invoice)
                .Where(a => a.Status == "Completed");

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);

            var serviceRevenue = await query
                .GroupBy(a => a.Service)
                .Select(g => new 
                {
                    Service = g.Key,
                    Revenue = g.Sum(a => a.Invoice != null ? a.Invoice.TotalAmount : 0),
                    Count = g.Count()
                })
                .OrderByDescending(x => x.Revenue)
                .ToListAsync();

            return serviceRevenue.Cast<object>().ToList();
        }
    }
}