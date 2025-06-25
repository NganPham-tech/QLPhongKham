using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data; // ADD THIS LINE
using QLPhongKham.Models;
using System.Collections.Generic;
using System.Linq;

namespace QLPhongKham.Repositories
{
    public class EFPatientRepository : Repository<Patient>, IPatientRepository
    {
        public EFPatientRepository(ApplicationDbContext context) : base(context) { }        public Patient? GetByEmail(string email)
        {
            return _dbSet.FirstOrDefault(p => p.Email == email);
        }

        public IQueryable<Patient> GetPatientsWithAppointments()
        {
            return _dbSet.Include(p => p.Appointments)
                         .ThenInclude(a => a.Doctor)
                         .Include(p => p.MedicalRecords);
        }

        public IQueryable<Patient> SearchPatients(string searchTerm)
        {
            return _dbSet.Where(p => p.FirstName.Contains(searchTerm) ||
                                   p.LastName.Contains(searchTerm) ||
                                   p.Email.Contains(searchTerm) ||
                                   p.Phone.Contains(searchTerm));
        }        public override Patient GetById(int id)
        {
            return _dbSet.Include(p => p.Appointments)
                         .ThenInclude(a => a.Doctor)
                         .Include(p => p.MedicalRecords)
                         .Include(p => p.Invoices)
                         .FirstOrDefault(p => p.PatientId == id) ?? throw new InvalidOperationException($"Patient with ID {id} not found");
        }

        public override IQueryable<Patient> GetAll()
        {
            return _dbSet.Include(p => p.Appointments);
        }
    }
}