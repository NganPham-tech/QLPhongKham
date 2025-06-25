using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data; // ADD THIS LINE
using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{
    public class EFDoctorRepository : Repository<Doctor>, IDoctorRepository
    {
        public EFDoctorRepository(ApplicationDbContext context) : base(context) { }

        public IQueryable<Doctor> GetDoctorsWithAppointments()
        {
            return _dbSet.Include(d => d.Appointments)
                         .ThenInclude(a => a.Patient);
        }        public Doctor? GetByEmail(string email)
        {
            return _dbSet.FirstOrDefault(d => d.Email == email);
        }        public IQueryable<Doctor> GetBySpecialty(string specialty)
        {
            return _dbSet.Where(d => d.Specialty != null && d.Specialty.Contains(specialty));
        }        public override Doctor GetById(int id)
        {
            return _dbSet.Include(d => d.Appointments)
                         .ThenInclude(a => a.Patient)
                         .Include(d => d.MedicalRecords)
                         .FirstOrDefault(d => d.DoctorId == id) ?? throw new InvalidOperationException($"Doctor with ID {id} not found");
        }

        public override IQueryable<Doctor> GetAll()
        {
            return _dbSet.Include(d => d.Appointments);
        }
    }
}