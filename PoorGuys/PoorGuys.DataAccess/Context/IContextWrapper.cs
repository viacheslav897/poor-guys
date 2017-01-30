using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace PoorGuys.DataAccess.Context
{
    public interface IContextWrapper
    {
        void SetState(object entity, EntityState state);
        void ApplyChanges();
        void RollBack();
        void CommitTransaction(bool startNewTransaction = false);
        IQueryable<TR> GetAll<TR>() where TR : class;
        void RemoveRange<TR>(IEnumerable<TR> entities) where TR : class;
    }
}