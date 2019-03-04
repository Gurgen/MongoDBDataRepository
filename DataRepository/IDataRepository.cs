using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using SmartArmenia.DataRepository.Models;

namespace SmartArmenia.DataRepository
{
    public interface IDataRepository
    {
        void AddEntity<T>(T entity) where T : BaseEntity;
        T GetEntityById<T>(string id, bool all = true) where T : BaseEntity;
        List<T> GetAll<T>(bool all = true) where T : BaseEntity;
        IQueryable<T> GetQueryable<T>(bool all = true, long date = 0) where T : BaseEntity;
        T UpdateEntity<T>(T entity) where T : BaseEntity;
        void DeleteEntity<T>(string entityId, bool force = false) where T : BaseEntity;
        void UpdateField<T, TField>(string id, Expression<Func<T, TField>> field, TField value) where T : BaseEntity;

        void AddItemToList<T, TField>(string entityId, Expression<Func<T, IEnumerable<TField>>> list, TField item)
            where T : BaseEntity;

        void DeleteItemFromList<T, TItem>(string entityId, Expression<Func<T, IEnumerable<TItem>>> field,
            Expression<Func<TItem, bool>> filter) where T : BaseEntity;
    }
}