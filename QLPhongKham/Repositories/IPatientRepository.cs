using QLPhongKham.Models;
using System.Collections.Generic;
namespace QLPhongKham.Repositories
{    public interface IPatientRepository : IRepository<Patient>
    {
        Patient? GetByEmail(string email);
        IQueryable<Patient> GetPatientsWithAppointments();
        IQueryable<Patient> SearchPatients(string searchTerm);
    }
}
