using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace PoorGuys.DataAccess.Context
{
    public class ContextWrapper<T> : IContextWrapper where T : DbContext
    {
        private IsolationLevel _isolationLevel = IsolationLevel.Unspecified;

        private DbTransaction _transaction;

        private DbConnection _connection;

        private bool _useTransaction = true;

        private string _connectionString;

        private DbContext _context;

        public void CommitTransaction(bool startNewTransaction = false)
        {
            if (_useTransaction)
            {
                if (_transaction?.Connection != null)
                {
                    _transaction.Commit();
                }

                if (startNewTransaction)
                {
                    StartTransaction();
                }
            }
        }

        internal void StartTransaction()
        {
            if (_useTransaction)
            {
                if (_transaction?.Connection == null)
                {
                    _transaction = _connection.BeginTransaction(_isolationLevel);
                }

            }
        }

        public void RollBack()
        {
            if (_useTransaction)
            {
                if (_transaction?.Connection != null)
                {
                    _transaction.Rollback();
                }
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

        public void SaveChanges()
        {
            _context?.SaveChanges();
        }

        public void SetState(object entity, EntityState state)
        {
            _context.Entry(entity).State = state;
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


        private void CreateContextInstance()
        {
            _context = !string.IsNullOrEmpty(_connectionString)
                ? (DbContext)Activator.CreateInstance(typeof(T), _connectionString)
                : (DbContext)Activator.CreateInstance(typeof(T));
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
