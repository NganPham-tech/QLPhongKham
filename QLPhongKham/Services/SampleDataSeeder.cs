using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Services
{    public static class SampleDataSeeder
    {
        public static async Task SeedSampleDataAsync(ApplicationDbContext context)
        {            // Đảm bảo có ít nhất 1 bác sĩ
            var doctor = await context.Doctors.FirstOrDefaultAsync();
            if (doctor == null)
            {
                doctor = new Doctor
                {
                    FirstName = "Dr. John",
                    LastName = "Smith",
                    DateOfBirth = new DateTime(1980, 1, 1),
                    Gender = "Nam",
                    Specialty = "Tổng quát",
                    DateHired = new DateTime(2020, 1, 1),
                    Phone = "0123456789",
                    Email = "doctor@clinic.com"
                };
                context.Doctors.Add(doctor);
                await context.SaveChangesAsync();
            }

            // Đảm bảo có ít nhất 1 dịch vụ
            var service = await context.Services.FirstOrDefaultAsync();
            if (service == null)
            {
                service = new Service
                {
                    Name = "Khám tổng quát",
                    Description = "Khám sức khỏe tổng quát",
                    Price = 200000,
                    Duration = 30
                };
                context.Services.Add(service);
                await context.SaveChangesAsync();
            }

            // Đảm bảo có ít nhất 1 nhân viên (Staff)
            var staff = await context.Staffs.FirstOrDefaultAsync();
            if (staff == null)
            {
                staff = new Staff
                {
                    FirstName = "Admin",
                    LastName = "User",
                    DateOfBirth = new DateTime(1990, 1, 1),
                    Gender = "Nam",
                    DateHired = DateTime.Now,
                    Position = "Quản lý",
                    Email = "admin@clinic.com",
                    Phone = "0987654321"
                };
                context.Staffs.Add(staff);
                await context.SaveChangesAsync();
            }

            // Kiểm tra xem đã có bệnh nhân Mei Chan chưa
            var existingPatient = await context.Patients
                .FirstOrDefaultAsync(p => p.Email == "mei@gmail.com");

            if (existingPatient == null)
            {
                // Tạo bệnh nhân mới
                var meiChan = new Patient
                {
                    FirstName = "Mei",
                    LastName = "Chan",
                    DateOfBirth = new DateTime(1995, 3, 15),
                    Gender = "Nữ",
                    Address = "123 Đường ABC, Quận 1, TP.HCM",
                    Phone = "0901234567",
                    Email = "mei@gmail.com"
                };

                context.Patients.Add(meiChan);
                await context.SaveChangesAsync();

                // Tạo lịch hẹn mẫu cho ngày 23/6/2025
                var appointment = new Appointment
                {
                    PatientId = meiChan.PatientId,
                    DoctorId = doctor.DoctorId,
                    ServiceId = service.ServiceId,
                    AppointmentDate = new DateTime(2025, 6, 23, 10, 0, 0),
                    Status = "Completed",
                    Notes = "Khám tổng quát định kỳ"
                };

                context.Appointments.Add(appointment);
                await context.SaveChangesAsync();
            }
        }
    }
}
