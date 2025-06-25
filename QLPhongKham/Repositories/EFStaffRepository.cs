using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data; // ADD THIS LINE
using QLPhongKham.Models;
using System.Collections.Generic;
using System.Linq;

namespace QLPhongKham.Repositories
{
    public class EFStaffRepository : Repository<Staff>, IStaffRepository
    {
        public EFStaffRepository(ApplicationDbContext context) : base(context) { }        public Staff? GetByEmail(string email)
        {
            return _dbSet.FirstOrDefault(s => s.Email == email);
        }

        public IQueryable<Staff> GetByPosition(string position)
        {
            return _dbSet.Where(s => s.Position == position);
        }        public override Staff GetById(int id)
        {
            return _dbSet.Include(s => s.InvoicesHandled)
                         .Include(s => s.InventoriesManaged)
                         .FirstOrDefault(s => s.StaffId == id) ?? throw new InvalidOperationException($"Staff with ID {id} not found");
        }
    }
}