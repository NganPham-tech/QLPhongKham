using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{
    public interface IAppointmentRepository : IRepository<Appointment>
    {
        IQueryable<Appointment> GetAppointmentsWithDetails();
        IQueryable<Appointment> GetAppointmentsByPatient(int patientId);
        IQueryable<Appointment> GetAppointmentsByDoctor(int doctorId);
        IQueryable<Appointment> GetTodayAppointments();
        IQueryable<Appointment> GetAppointmentsByDate(DateTime date);
    }
}
