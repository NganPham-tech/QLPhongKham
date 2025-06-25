using QLPhongKham.Models;
using System.Collections.Generic;
namespace QLPhongKham.Repositories
{    public interface IStaffRepository : IRepository<Staff>
    {
        Staff? GetByEmail(string email);
        IQueryable<Staff> GetByPosition(string position);
    }
}
