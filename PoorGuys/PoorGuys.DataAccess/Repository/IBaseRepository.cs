using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace PoorGuys.DataAccess.Repository
{
    interface IBaseRepository<T> where T: class
    {
        IQueryable<T> GetAll(params Expression<Func<T, object>>[] include);
        T Add(T entity, params Expression<Func<T, object>>[] include);
        void AddRange(IEnumerable<T> entities);
        T Update(T entity, params Expression<Func<T, object>>[] include);
        void UpdateRange(IEnumerable<T> entities);
        void Delete(T entity, params Expression<Func<T, object>>[] include);
        void RemoveRange(IEnumerable<T> entities);
        void CommitTransaction();
        void RollbackTransaction();

    }
}
