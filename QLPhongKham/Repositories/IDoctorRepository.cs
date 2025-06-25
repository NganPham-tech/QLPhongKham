using QLPhongKham.Models;
using System.Collections.Generic;
namespace QLPhongKham.Repositories
{    public interface IDoctorRepository : IRepository<Doctor>
    {
        IQueryable<Doctor> GetDoctorsWithAppointments();
        Doctor? GetByEmail(string email);
        IQueryable<Doctor> GetBySpecialty(string specialty);
    }
}
