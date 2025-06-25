using QLPhongKham.Data; 
using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{
    public class ServiceRepository : Repository<Service>, IServiceRepository
    {
        public ServiceRepository(ApplicationDbContext context) : base(context) { }

        public IQueryable<Service> GetActiveServices()
        {
            // Có thể thêm điều kiện IsActive nếu cần
            return _dbSet.Where(s => s.Price > 0);
        }
    }
}