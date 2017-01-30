using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore;
using PoorGuys.DataAccess.Context;

namespace PoorGuys.DataAccess.Repository
{
    internal class BaseRepository<T> : IBaseRepository<T> where T: class 
    {
        public BaseRepository(IContextWrapper contextBase)
        {
            ContextBase = contextBase;
        }

        protected IContextWrapper ContextBase { get; set; }

        public IQueryable<T> GetAll(params Expression<Func<T, object>>[] include)
        {
            var result = ContextBase.GetAll<T>();
            if (include != null)
            {
                result = include.Aggregate(result, (current, expression) => current.Include(expression));
            }

            return result;
        }

        public T Add(T entity, params Expression<Func<T, object>>[] include)
        {
            return SetStateForEntities(entity, include, EntityState.Added);
        }

        public void AddRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                ContextBase.SetState(entity, EntityState.Added);
            }

            ContextBase.ApplyChanges();
        }

        public T Update(T entity, params Expression<Func<T, object>>[] include)
        {
            return SetStateForEntities(entity, include, EntityState.Modified);
        }

        public void UpdateRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                ContextBase.SetState(entity, EntityState.Modified);
            }

            ContextBase.ApplyChanges();
        }

        public void Delete(T entity, params Expression<Func<T, object>>[] include)
        {
            SetStateForEntities(entity, include, EntityState.Deleted);
        }

        public void RemoveRange(IEnumerable<T> entities)
        {
            ContextBase.RemoveRange(entities);
        }

        public void CommitTransaction()
        {
            ContextBase.CommitTransaction();
        }

        public void RollbackTransaction()
        {
            ContextBase.RollBack();
        }

        private T SetStateForEntities(T obj, IEnumerable<Expression<Func<T, object>>> include, EntityState state)
        {
            foreach (var property in include)
            {
                object value = property.Compile()(obj);
                Type listType = GetEnumerableType(value.GetType());
                if (listType != null)
                {
                    var listProperty = value as IEnumerable;
                    if (listProperty != null)
                    {
                        foreach (object prop in listProperty)
                        {
                            ContextBase.SetState(prop, state);
                        }
                    }
                }
                else
                {
                    ContextBase.SetState(value, state);
                }
            }

            ContextBase.SetState(obj, state);
            ContextBase.ApplyChanges();

            return obj;
        }

        private Type GetEnumerableType(Type type)
        {
            foreach (var intType in type.GetInterfaces())
            {
                if (intType.IsGenericType && intType.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                {
                    return intType.GetGenericArguments()[0];
                }
            }

            return null;
        }
    }
}