using Microsoft.EntityFrameworkCore;
using QLPhongKham.Models;
using QLPhongKham.Data;

namespace QLPhongKham.TestTools
{
    public class DatabaseChecker
    {
        public static async Task CheckMeiChanAppointments(ApplicationDbContext context)
        {
            Console.WriteLine("=== CHECKING MEI CHAN APPOINTMENTS ===");
            
            // Find Mei Chan
            var meiChan = await context.Patients
                .Where(p => p.Email == "mei@gmail.com")
                .FirstOrDefaultAsync();
                
            if (meiChan == null)
            {
                Console.WriteLine("❌ Mei Chan not found!");
                return;
            }
            
            Console.WriteLine($"✅ Found Mei Chan: {meiChan.FirstName} {meiChan.LastName} (ID: {meiChan.PatientId})");
            
            // Find her appointments
            var appointments = await context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .Where(a => a.PatientId == meiChan.PatientId)
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
                
            Console.WriteLine($"📅 Total appointments for Mei Chan: {appointments.Count}");
            
            foreach (var apt in appointments)
            {
                Console.WriteLine($"  - {apt.AppointmentDate:dd/MM/yyyy HH:mm} | Status: {apt.Status} | Service: {apt.Service?.Name} | Doctor: {apt.Doctor?.FirstName} {apt.Doctor?.LastName}");
            }
            
            // Check June 23, 2025 specifically
            var june23 = new DateTime(2025, 6, 23);
            var june23Appointments = appointments.Where(a => a.AppointmentDate.Date == june23).ToList();
            
            Console.WriteLine($"\n🎯 Appointments on June 23, 2025: {june23Appointments.Count}");
            foreach (var apt in june23Appointments)
            {
                Console.WriteLine($"  - ID: {apt.AppointmentId} | {apt.AppointmentDate:dd/MM/yyyy HH:mm} | Status: {apt.Status}");
                
                // Check if this appointment has an invoice
                var invoice = await context.Invoices.FirstOrDefaultAsync(i => i.AppointmentId == apt.AppointmentId);
                if (invoice != null)
                {
                    Console.WriteLine($"    ⚠️ Already has invoice: {invoice.InvoiceNumber}");
                }
                else
                {
                    Console.WriteLine($"    ✅ No invoice - ready for invoice creation!");
                }
            }
        }
        
        public static async Task CheckCompletedAppointmentsWithoutInvoices(ApplicationDbContext context)
        {
            Console.WriteLine("\n=== COMPLETED APPOINTMENTS WITHOUT INVOICES ===");
            
            var completedWithoutInvoice = await context.Appointments
                .Include(a => a.Patient)
                .Include(a => a.Service)
                .Include(a => a.Doctor)
                .Where(a => a.Status == "Completed" && 
                           !context.Invoices.Any(i => i.AppointmentId == a.AppointmentId))
                .OrderByDescending(a => a.AppointmentDate)
                .ToListAsync();
                
            Console.WriteLine($"📋 Total completed appointments without invoices: {completedWithoutInvoice.Count}");
            
            foreach (var apt in completedWithoutInvoice)
            {
                var dateInfo = apt.AppointmentDate.Date == new DateTime(2025, 6, 23) ? " 🎯 JUNE 23!" : "";
                Console.WriteLine($"  - {apt.Patient?.FirstName} {apt.Patient?.LastName} | {apt.AppointmentDate:dd/MM/yyyy HH:mm} | {apt.Service?.Name}{dateInfo}");
            }
        }
    }
}
