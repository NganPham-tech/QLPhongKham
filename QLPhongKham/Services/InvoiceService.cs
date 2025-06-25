using Microsoft.EntityFrameworkCore;
using QLPhongKham.Models;
using QLPhongKham.Data;

namespace QLPhongKham.Services
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;

        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Invoice> CreateInvoiceFromAppointmentAsync(int appointmentId, int staffId)
        {
            var appointment = await _context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .FirstOrDefaultAsync(a => a.AppointmentId == appointmentId);

            if (appointment == null)
                throw new ArgumentException("Appointment not found");

            // Check if invoice already exists for this appointment
            var existingInvoice = await _context.Invoices
                .FirstOrDefaultAsync(i => i.AppointmentId == appointmentId);

            if (existingInvoice != null)
                return existingInvoice;

            var invoice = new Invoice
            {
                PatientId = appointment.PatientId,
                AppointmentId = appointmentId,
                InvoiceNumber = await GenerateInvoiceNumberAsync(),
                CreatedDate = DateTime.Now,
                DueDate = DateTime.Now.AddDays(30),
                StaffId = staffId,
                Status = "Draft",
                InvoiceType = "Service",
                SubTotal = appointment.Service?.Price ?? 0,
                TotalAmount = appointment.Service?.Price ?? 0,
                TaxAmount = 0,
                DiscountAmount = 0,
                PaidAmount = 0
            };

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            return invoice;
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int invoiceId)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Appointment)
                .Include(i => i.Staff)
                .Include(i => i.InvoiceDetails)
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByPatientIdAsync(int patientId)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Payments)
                .Where(i => i.PatientId == patientId)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesByStatusAsync(string status)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Payments)
                .Where(i => i.Status == status)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Invoice>> GetOverdueInvoicesAsync()
        {
            var today = DateTime.Today;
            return await _context.Invoices
                .Include(i => i.Patient)
                .Where(i => i.DueDate.HasValue && i.DueDate < today && i.RemainingAmount > 0)
                .OrderBy(i => i.DueDate)
                .ToListAsync();
        }

        public async Task<Invoice?> UpdateInvoiceAsync(Invoice invoice)
        {
            invoice.UpdatedDate = DateTime.Now;
            _context.Invoices.Update(invoice);
            await _context.SaveChangesAsync();
            return invoice;
        }

        public async Task<bool> DeleteInvoiceAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Payments)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null) return false;

            // Cannot delete invoice with payments
            if (invoice.Payments.Any()) return false;

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<string> GenerateInvoiceNumberAsync()
        {
            var today = DateTime.Today;
            var prefix = $"INV{today:yyyyMMdd}";
            
            var lastInvoice = await _context.Invoices
                .Where(i => i.InvoiceNumber.StartsWith(prefix))
                .OrderByDescending(i => i.InvoiceNumber)
                .FirstOrDefaultAsync();

            int nextNumber = 1;
            if (lastInvoice != null)
            {
                var lastNumberStr = lastInvoice.InvoiceNumber.Substring(prefix.Length);
                if (int.TryParse(lastNumberStr, out int lastNumber))
                {
                    nextNumber = lastNumber + 1;
                }
            }

            return $"{prefix}-{nextNumber:D3}";
        }

        public async Task<decimal> CalculateInvoiceTotalAsync(int invoiceId)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            if (invoice == null) return 0;

            var subTotal = invoice.InvoiceDetails.Sum(id => id.UnitPrice * id.Quantity);
            var taxAmount = subTotal * 0.1m; // 10% VAT
            var totalAmount = subTotal + taxAmount - invoice.DiscountAmount;

            invoice.SubTotal = subTotal;
            invoice.TaxAmount = taxAmount;
            invoice.TotalAmount = totalAmount;

            await _context.SaveChangesAsync();
            return totalAmount;
        }

        public async Task<Invoice?> AddServiceToInvoiceAsync(int invoiceId, int serviceId, int quantity = 1)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            var service = await _context.Services.FindAsync(serviceId);

            if (invoice == null || service == null)
                throw new ArgumentException("Invoice or Service not found");

            // Check if service already exists in invoice
            var existingDetail = invoice.InvoiceDetails
                .FirstOrDefault(id => id.ServiceId == serviceId);
            
            if (existingDetail != null)
            {
                existingDetail.Quantity += quantity;
            }
            else
            {
                var invoiceDetail = new InvoiceDetail
                {
                    Invoice = invoice,
                    InvoiceId = invoiceId,
                    ServiceId = serviceId,
                    UnitPrice = service.Price,
                    Quantity = quantity
                };

                _context.InvoiceDetails.Add(invoiceDetail);
            }

            await _context.SaveChangesAsync();
            await CalculateInvoiceTotalAsync(invoiceId);

            return invoice;
        }

        public async Task<Invoice?> AddMedicineToInvoiceAsync(int invoiceId, int medicineId, int quantity)
        {
            var invoice = await _context.Invoices
                .Include(i => i.InvoiceDetails)
                .FirstOrDefaultAsync(i => i.InvoiceId == invoiceId);

            var medicine = await _context.Medicines.FindAsync(medicineId);

            if (invoice == null || medicine == null)
                throw new ArgumentException("Invoice or Medicine not found");

            // Check if medicine already exists in invoice
            var existingDetail = invoice.InvoiceDetails
                .FirstOrDefault(id => id.MedicineId == medicineId);

            if (existingDetail != null)
            {
                existingDetail.Quantity += quantity;
            }
            else
            {
                var invoiceDetail = new InvoiceDetail
                {
                    Invoice = invoice,
                    InvoiceId = invoiceId,
                    MedicineId = medicineId,
                    UnitPrice = medicine.UnitPrice,
                    Quantity = quantity
                };

                _context.InvoiceDetails.Add(invoiceDetail);
            }

            await _context.SaveChangesAsync();
            await CalculateInvoiceTotalAsync(invoiceId);

            return invoice;
        }

        public async Task<bool> MarkAsPaidAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = "Paid";
            invoice.PaidAmount = invoice.TotalAmount;
            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> MarkAsOverdueAsync(int invoiceId)
        {
            var invoice = await _context.Invoices.FindAsync(invoiceId);
            if (invoice == null) return false;

            invoice.Status = "Overdue";
            invoice.UpdatedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Invoice>> GetInvoicesForFinancialReportAsync(DateTime fromDate, DateTime toDate)
        {
            return await _context.Invoices
                .Include(i => i.Patient)
                .Include(i => i.Payments)
                .Where(i => i.CreatedDate >= fromDate && i.CreatedDate <= toDate)
                .OrderByDescending(i => i.CreatedDate)
                .ToListAsync();
        }

        public async Task<Invoice> CreateInvoiceAsync(int patientId, int staffId, string invoiceType = "Service")
        {
            try
            {
                Console.WriteLine($"Creating invoice - PatientId: {patientId}, StaffId: {staffId}, Type: {invoiceType}");

                // Validate patient exists
                var patient = await _context.Patients.FindAsync(patientId);
                if (patient == null)
                    throw new ArgumentException($"Patient with ID {patientId} not found");

                Console.WriteLine($"Patient found: {patient.FirstName} {patient.LastName}");

                // Validate staff exists (if staffId > 0)
                Staff? staff = null;
                if (staffId > 0)
                {
                    staff = await _context.Staffs.FindAsync(staffId);
                    if (staff == null)
                        Console.WriteLine($"Warning: Staff with ID {staffId} not found, creating invoice without staff");
                    else
                        Console.WriteLine($"Staff found: {staff.FirstName} {staff.LastName}");
                }

                var invoiceNumber = await GenerateInvoiceNumberAsync();
                Console.WriteLine($"Generated invoice number: {invoiceNumber}");

                var invoice = new Invoice
                {
                    PatientId = patientId,
                    AppointmentId = null, // No appointment for manual invoices
                    InvoiceNumber = invoiceNumber,
                    CreatedDate = DateTime.Now,
                    DueDate = DateTime.Now.AddDays(30), // 30 days payment term
                    StaffId = staff != null ? staffId : null,
                    Status = "Draft",
                    InvoiceType = invoiceType,
                    SubTotal = 0,
                    TotalAmount = 0,
                    TaxAmount = 0,
                    DiscountAmount = 0,
                    PaidAmount = 0,
                    Notes = $"Invoice created manually on {DateTime.Now:yyyy-MM-dd HH:mm}"
                };

                Console.WriteLine("Adding invoice to context...");
                _context.Invoices.Add(invoice);
                
                Console.WriteLine("Saving changes...");
                await _context.SaveChangesAsync();
                
                Console.WriteLine($"Invoice created successfully with ID: {invoice.InvoiceId}");
                return invoice;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in CreateInvoiceAsync: {ex.Message}");
                Console.WriteLine($"Inner exception: {ex.InnerException?.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw new InvalidOperationException($"Error creating invoice: {ex.Message}", ex);
            }
        }
    }
}
