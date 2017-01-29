using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
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

        private IQueryable<TR> SetEntities<TR>() where TR : class
        {
            var entities = _context.Set<TR>();

            return entities;
        }


        private void CreateContextInstance()
        {
            _context = !string.IsNullOrEmpty(_connectionString)
                ? (DbContext)Activator.CreateInstance(typeof(T), _connectionString)
                : (DbContext)Activator.CreateInstance(typeof(T));
            _context.
        }
    }
}
