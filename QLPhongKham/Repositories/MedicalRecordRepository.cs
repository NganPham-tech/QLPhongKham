using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data; // ADD THIS LINE
using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{
    public class MedicalRecordRepository : Repository<MedicalRecord>, IMedicalRecordRepository
    {
        public MedicalRecordRepository(ApplicationDbContext context) : base(context) { }

        public IQueryable<MedicalRecord> GetMedicalRecordsWithDetails()
        {
            return _dbSet.Include(m => m.Patient)
                         .Include(m => m.Appointment)
                         .ThenInclude(a => a.Doctor);
        }

        public IQueryable<MedicalRecord> GetMedicalRecordsByPatient(int patientId)
        {
            return GetMedicalRecordsWithDetails().Where(m => m.PatientId == patientId);
        }        public MedicalRecord? GetByAppointmentId(int appointmentId)
        {
            return GetMedicalRecordsWithDetails().FirstOrDefault(m => m.AppointmentId == appointmentId);
        }

        public override MedicalRecord? GetById(int id)
        {
            return GetMedicalRecordsWithDetails().FirstOrDefault(m => m.MedicalRecordId == id);
        }

        public override IQueryable<MedicalRecord> GetAll()
        {
            return GetMedicalRecordsWithDetails();
        }
    }
}