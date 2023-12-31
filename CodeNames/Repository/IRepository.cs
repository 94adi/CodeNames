using System.Linq.Expressions;

namespace CodeNames.Repository
{
    public interface IRepository<T> where T : class
    {
        T Get(int id);

        T Get(int id, Expression<Func<T, bool>> filter, string includeProperties);

        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null,
            Func<IQueryable<T>, IOrderedQueryable<T>> orderBy = null,
            string includeProperties = null
            );

        T GetFirstOrDefault(
            Expression<Func<T, bool>> filter = null,
            string includeProperties = null
            );

        void Add(T entity);
        void Remove(T entity);
        void Remove(int id);
        void RemoveRange(IEnumerable<T> entities);
        void Save();
    }
}
