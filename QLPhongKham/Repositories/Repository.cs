using Microsoft.EntityFrameworkCore;
using QLPhongKham.Data; // ADD THIS LINE
using QLPhongKham.Models;

namespace QLPhongKham.Repositories
{
    public class Repository<T> : IRepository<T> where T : class
    {
        protected readonly ApplicationDbContext _context;
        protected readonly DbSet<T> _dbSet;

        public Repository(ApplicationDbContext context)
        {
            _context = context;
            _dbSet = context.Set<T>();
        }

        public virtual IQueryable<T> GetAll()
        {
            return _dbSet;
        }        public virtual T? GetById(int id)
        {
            return _dbSet.Find(id);
        }

        public virtual void Add(T entity)
        {
            _dbSet.Add(entity);
            Save();
        }

        public virtual void Update(T entity)
        {
            _dbSet.Update(entity);
            Save();
        }

        public virtual void Delete(T entity)
        {
            _dbSet.Remove(entity);
            Save();
        }

        public virtual void Save()
        {
            _context.SaveChanges();
        }
    }
}
