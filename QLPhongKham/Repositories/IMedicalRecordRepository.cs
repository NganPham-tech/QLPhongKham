using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{    public interface IMedicalRecordRepository : IRepository<MedicalRecord>
    {
        IQueryable<MedicalRecord> GetMedicalRecordsWithDetails();
        IQueryable<MedicalRecord> GetMedicalRecordsByPatient(int patientId);
        MedicalRecord? GetByAppointmentId(int appointmentId);
    }
}
