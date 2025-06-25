using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{
    public interface IServiceRepository : IRepository<Service>
    {
        IQueryable<Service> GetActiveServices();
    }
}
