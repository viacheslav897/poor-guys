using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace PoorGuys.DataAccess.Context
{
    public class ContextWrapper : IContextWrapper
    {
        private readonly DbContext _context;

        private IDbContextTransaction _transaction;

        private readonly bool _useTransaction = true;

        public ContextWrapper(DbContext context)
        {
            _context = context;
        }

        public void CommitTransaction(bool startNewTransaction = false)
        {
            if (_useTransaction)
            {
                _transaction?.Commit();

                if (startNewTransaction)
                {
                    StartTransaction();
                }
            }
        }

        public void RollBack()
        {
            if (_useTransaction)
            {
                _transaction?.Rollback();
            }
        }

        public IQueryable<TR> GetAll<TR>() where TR : class
        {
            var entities = SetEntities<TR>().AsNoTracking();

            return entities;
        }

        public void RemoveRange<TR>(IEnumerable<TR> entities) where TR : class
        {
            _context.RemoveRange(entities);
        }

        public void ApplyChanges()
        {
            ProcessTransactionableMethod(
                () =>
                {
                    try
                    {
                        SaveChanges();
                    }
                    catch (Exception)
                    {
                        RollBack();
                        throw;
                    }
                });
        }

        public void SetState(object entity, EntityState state)
        {
            _context.Entry(entity).State = state;
        }

        internal void StartTransaction()
        {
            if (_useTransaction)
            {
                if (_transaction == null)
                {
                    _transaction = _context.Database.BeginTransaction();
                }
            }
        }

        public void SaveChanges()
        {
            _context?.SaveChanges();
        }

        private void ProcessTransactionableMethod(Action action)
        {
            StartTransaction();
            action();
        }

        private IQueryable<TR> SetEntities<TR>() where TR : class
        {
            var entities = _context.Set<TR>();

            return entities;
        }

        private IQueryable<TR> ApplyIncludesToQuery<TR>(
            IQueryable<TR> entities,
            Expression<Func<TR, object>>[] includes) where TR : class
        {
            if (includes != null)
            {
                entities = includes.Aggregate(entities, (current, include) => current.Include(include));
            }

            return entities;
        }

        private void SetUnchangedTrakingEntities()
        {
            var trakingEntities = _context.ChangeTracker.Entries();
            foreach (var trakingEntity in trakingEntities)
            {
                trakingEntity.State = EntityState.Unchanged;
            }
        }

        private void DetachAllUnchangedEntities()
        {
            var objectStateEntries =
                _context.ChangeTracker.Entries().Where(e => e.State == EntityState.Unchanged);
            foreach (var objectStateEntry in objectStateEntries)
            {
                objectStateEntry.State = EntityState.Detached;
            }
        }
    }
}