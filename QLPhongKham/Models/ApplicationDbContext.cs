// Data/ApplicationDbContext.cs - Fixed
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Models;

namespace QLPhongKham.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<Patient> Patients { get; set; }
        public DbSet<Doctor> Doctors { get; set; }
        public DbSet<Staff> Staffs { get; set; }
        public DbSet<Appointment> Appointments { get; set; }
        public DbSet<Service> Services { get; set; }
        public DbSet<MedicalRecord> MedicalRecords { get; set; }
        public DbSet<Medicine> Medicines { get; set; }
        public DbSet<Inventory> Inventories { get; set; }
        public DbSet<Invoice> Invoices { get; set; }
        public DbSet<InvoiceDetail> InvoiceDetails { get; set; }        public DbSet<Payment> Payments { get; set; }
        public DbSet<Notification> Notifications { get; set; }
        public DbSet<Article> Articles { get; set; }
        public DbSet<FAQ> FAQs { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Configure decimal precision for all monetary fields

            // Staff Salary
            modelBuilder.Entity<Staff>()
                .Property(s => s.Salary)
                .HasPrecision(18, 2);

            // Service Price
            modelBuilder.Entity<Service>()
                .Property(s => s.Price)
                .HasPrecision(18, 2);

            // Medicine Unit Price
            modelBuilder.Entity<Medicine>()
                .Property(m => m.UnitPrice)
                .HasPrecision(18, 2);

            // Invoice Total Amount
            modelBuilder.Entity<Invoice>()
                .Property(i => i.TotalAmount)
                .HasPrecision(18, 2);

            // Invoice Detail Unit Price
            modelBuilder.Entity<InvoiceDetail>()
                .Property(id => id.UnitPrice)
                .HasPrecision(18, 2);

            // Payment Amount Paid
            modelBuilder.Entity<Payment>()
                .Property(p => p.AmountPaid)
                .HasPrecision(18, 2);

            // Configure relationships with restricted delete behavior

            // Appointment relationships
            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Patient)
                .WithMany(p => p.Appointments)
                .HasForeignKey(a => a.PatientId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Doctor)
                .WithMany(d => d.Appointments)
                .HasForeignKey(a => a.DoctorId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Appointment>()
                .HasOne(a => a.Service)
                .WithMany(s => s.Appointments)
                .HasForeignKey(a => a.ServiceId)
                .OnDelete(DeleteBehavior.Cascade);

            // Medical Record relationships
            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Appointment)
                .WithOne(a => a.MedicalRecord)
                .HasForeignKey<MedicalRecord>(mr => mr.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Patient)
                .WithMany(p => p.MedicalRecords)
                .HasForeignKey(mr => mr.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<MedicalRecord>()
                .HasOne(mr => mr.Doctor)
                .WithMany(d => d.MedicalRecords)
                .HasForeignKey(mr => mr.DoctorId)
                .OnDelete(DeleteBehavior.SetNull);

            // Invoice relationships
            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Patient)
                .WithMany(p => p.Invoices)
                .HasForeignKey(i => i.PatientId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Appointment)
                .WithOne(a => a.Invoice)
                .HasForeignKey<Invoice>(i => i.AppointmentId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Invoice>()
                .HasOne(i => i.Staff)
                .WithMany(s => s.InvoicesHandled)
                .HasForeignKey(i => i.StaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Invoice Detail relationships
            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(id => id.Invoice)
                .WithMany(i => i.InvoiceDetails)
                .HasForeignKey(id => id.InvoiceId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(id => id.Service)
                .WithMany()
                .HasForeignKey(id => id.ServiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<InvoiceDetail>()
                .HasOne(id => id.Medicine)
                .WithMany(m => m.InvoiceDetails)
                .HasForeignKey(id => id.MedicineId)
                .OnDelete(DeleteBehavior.Restrict);            // Payment relationships
            modelBuilder.Entity<Payment>()
                .HasOne(p => p.Invoice)
                .WithMany(i => i.Payments)
                .HasForeignKey(p => p.InvoiceId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Payment>()
                .HasOne(p => p.CollectedByStaff)
                .WithMany(s => s.PaymentsCollected)
                .HasForeignKey(p => p.CollectedByStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            // Inventory relationships
            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.Medicine)
                .WithMany(m => m.Inventories)
                .HasForeignKey(i => i.MedicineId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Inventory>()
                .HasOne(i => i.ManagedByStaff)
                .WithMany(s => s.InventoriesManaged)
                .HasForeignKey(i => i.ManagedByStaffId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure indexes for better performance

            // Patient indexes
            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Email)
                .IsUnique();

            modelBuilder.Entity<Patient>()
                .HasIndex(p => p.Phone);

            // Doctor indexes
            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<Doctor>()
                .HasIndex(d => d.Specialty);

            // Staff indexes
            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Email)
                .IsUnique()
                .HasFilter("[Email] IS NOT NULL");

            modelBuilder.Entity<Staff>()
                .HasIndex(s => s.Department);

            // Appointment indexes
            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.AppointmentDate);

            modelBuilder.Entity<Appointment>()
                .HasIndex(a => a.Status);

            // Medicine indexes
            modelBuilder.Entity<Medicine>()
                .HasIndex(m => m.Name);

            // Service indexes
            modelBuilder.Entity<Service>()
                .HasIndex(s => s.Name);

            // Invoice indexes
            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.CreatedDate);

            modelBuilder.Entity<Invoice>()
                .HasIndex(i => i.Status);

            // Configure default values

            // Appointment default status
            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasDefaultValue("Scheduled");

            // Appointment default created date
            modelBuilder.Entity<Appointment>()
                .Property(a => a.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            // Invoice default created date
            modelBuilder.Entity<Invoice>()
                .Property(i => i.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            // Invoice default status
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasDefaultValue("Pending");

            // Medical Record default created date
            modelBuilder.Entity<MedicalRecord>()
                .Property(mr => mr.CreatedDate)
                .HasDefaultValueSql("GETDATE()");

            // Payment default date
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentDate)
                .HasDefaultValueSql("GETDATE()");

            // Payment default method
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasDefaultValue("Cash");

            // Service default active
            modelBuilder.Entity<Service>()
                .Property(s => s.IsActive)
                .HasDefaultValue(true);

            // Medicine default active and unit
            modelBuilder.Entity<Medicine>()
                .Property(m => m.IsActive)
                .HasDefaultValue(true);

            modelBuilder.Entity<Medicine>()
                .Property(m => m.Unit)
                .HasDefaultValue("Viên");

            // Inventory default import date
            modelBuilder.Entity<Inventory>()
                .Property(i => i.ImportDate)
                .HasDefaultValueSql("GETDATE()");

            // Configure string length limits for better database performance

            // Patient
            modelBuilder.Entity<Patient>()
                .Property(p => p.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Patient>()
                .Property(p => p.LastName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Patient>()
                .Property(p => p.Gender)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<Patient>()
                .Property(p => p.Address)
                .HasMaxLength(200);

            modelBuilder.Entity<Patient>()
                .Property(p => p.Phone)
                .HasMaxLength(20)
                .IsRequired();

            modelBuilder.Entity<Patient>()
                .Property(p => p.Email)
                .HasMaxLength(100)
                .IsRequired();

            // Doctor
            modelBuilder.Entity<Doctor>()
                .Property(d => d.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Doctor>()
                .Property(d => d.LastName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Gender)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Specialty)
                .HasMaxLength(200);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Email)
                .HasMaxLength(100);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.PhotoUrl)
                .HasMaxLength(200)
                .HasDefaultValue("/images/default-doctor.jpg");

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Description)
                .HasMaxLength(500);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Address)
                .HasMaxLength(200);

            modelBuilder.Entity<Doctor>()
                .Property(d => d.Qualification)
                .HasMaxLength(100);

            // Staff
            modelBuilder.Entity<Staff>()
                .Property(s => s.FirstName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Staff>()
                .Property(s => s.LastName)
                .HasMaxLength(100)
                .IsRequired();

            modelBuilder.Entity<Staff>()
                .Property(s => s.Gender)
                .HasMaxLength(10)
                .IsRequired();

            modelBuilder.Entity<Staff>()
                .Property(s => s.Address)
                .HasMaxLength(200);

            modelBuilder.Entity<Staff>()
                .Property(s => s.Phone)
                .HasMaxLength(20);

            modelBuilder.Entity<Staff>()
                .Property(s => s.Email)
                .HasMaxLength(100);

            modelBuilder.Entity<Staff>()
                .Property(s => s.Position)
                .HasMaxLength(50);

            modelBuilder.Entity<Staff>()
                .Property(s => s.Department)
                .HasMaxLength(50);

            modelBuilder.Entity<Staff>()
                .Property(s => s.PhotoUrl)
                .HasMaxLength(200)
                .HasDefaultValue("/images/default-staff.jpg");

            // Service
            modelBuilder.Entity<Service>()
                .Property(s => s.Name)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Service>()
                .Property(s => s.Description)
                .HasMaxLength(500)
                .IsRequired();

            // Medicine
            modelBuilder.Entity<Medicine>()
                .Property(m => m.Name)
                .HasMaxLength(150)
                .IsRequired();

            modelBuilder.Entity<Medicine>()
                .Property(m => m.Description)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<Medicine>()
                .Property(m => m.Unit)
                .HasMaxLength(50);

            modelBuilder.Entity<Medicine>()
                .Property(m => m.Manufacturer)
                .HasMaxLength(100);

            // Appointment
            modelBuilder.Entity<Appointment>()
                .Property(a => a.Status)
                .HasMaxLength(200)
                .IsRequired();

            modelBuilder.Entity<Appointment>()
                .Property(a => a.Notes)
                .HasMaxLength(500)
                .IsRequired();

            // Medical Record
            modelBuilder.Entity<MedicalRecord>()
                .Property(mr => mr.Diagnosis)
                .HasMaxLength(500)
                .IsRequired();

            modelBuilder.Entity<MedicalRecord>()
                .Property(mr => mr.Treatment)
                .HasMaxLength(1000)
                .IsRequired();

            modelBuilder.Entity<MedicalRecord>()
                .Property(mr => mr.Notes)
                .HasMaxLength(1000)
                .IsRequired();

            modelBuilder.Entity<MedicalRecord>()
                .Property(mr => mr.Prescription)
                .HasMaxLength(200);

            modelBuilder.Entity<MedicalRecord>()
                .Property(mr => mr.Advice)
                .HasMaxLength(200);

            // Invoice
            modelBuilder.Entity<Invoice>()
                .Property(i => i.Status)
                .HasMaxLength(20);

            modelBuilder.Entity<Invoice>()
                .Property(i => i.Notes)
                .HasMaxLength(200);

            // Invoice Detail
            modelBuilder.Entity<InvoiceDetail>()
                .Property(id => id.Notes)
                .HasMaxLength(200);            // Payment
            modelBuilder.Entity<Payment>()
                .Property(p => p.PaymentMethod)
                .HasMaxLength(50)
                .IsRequired();

            modelBuilder.Entity<Payment>()
                .Property(p => p.BankTransactionId)
                .HasMaxLength(100);

            modelBuilder.Entity<Payment>()
                .Property(p => p.Notes)
                .HasMaxLength(500);

            // Inventory
            modelBuilder.Entity<Inventory>()
                .Property(i => i.Notes)
                .HasMaxLength(200);
        }
    }
}