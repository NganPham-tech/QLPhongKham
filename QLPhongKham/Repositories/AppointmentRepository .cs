
using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data;
using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{
    public class AppointmentRepository : Repository<Appointment>, IAppointmentRepository
    {
        public AppointmentRepository(ApplicationDbContext context) : base(context) { } 

        public IQueryable<Appointment> GetAppointmentsWithDetails()
        {
            return _dbSet.Include(a => a.Patient)
                         .Include(a => a.Doctor)
                         .Include(a => a.Service);
        }

        public IQueryable<Appointment> GetAppointmentsByPatient(int patientId)
        {
            return GetAppointmentsWithDetails().Where(a => a.PatientId == patientId);
        }

        public IQueryable<Appointment> GetAppointmentsByDoctor(int doctorId)
        {
            return GetAppointmentsWithDetails().Where(a => a.DoctorId == doctorId);
        }

        public IQueryable<Appointment> GetTodayAppointments()
        {
            return GetAppointmentsWithDetails().Where(a => a.AppointmentDate.Date == DateTime.Today);
        }

        public IQueryable<Appointment> GetAppointmentsByDate(DateTime date)
        {
            return GetAppointmentsWithDetails().Where(a => a.AppointmentDate.Date == date.Date);
        }        public override Appointment? GetById(int id)
        {
            return GetAppointmentsWithDetails().FirstOrDefault(a => a.AppointmentId == id);
        }

        public override IQueryable<Appointment> GetAll()
        {
            return GetAppointmentsWithDetails();
        }
    }
}