using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver;
using MongoDB.Driver.Linq;
using SmartArmenia.DataRepository.Models;

namespace SmartArmenia.DataRepository
{
    public class MongoDbDataRepository : IDataRepository
    {
        private readonly IMongoDatabase _database;

        public MongoDbDataRepository(string connectionString, string database)
        {
            IMongoClient client = new MongoClient(connectionString);
            _database = client.GetDatabase(database);
        }

        public void AddEntity<T>(T entity) where T : BaseEntity
        {
            entity.LastUpdate = DateTime.UtcNow.Ticks;
            _database.GetCollection<T>(GetCollectionName<T>()).InsertOne(entity);
        }

        public T GetEntityById<T>(string id, bool all = true) where T : BaseEntity
        {
            try
            {
                return GetQueryable<T>(all).FirstOrDefault(x => x.Id == id);
            }
            catch
            {
                return null;
            }
        }

        public List<T> GetAll<T>(bool all = true) where T : BaseEntity
        {
            var query = _database.GetCollection<T>(GetCollectionName<T>()).AsQueryable();
            if (!all)
            {
                query = query.Where(x => x.Active);
            }

            return query.ToList();
        }

        public IQueryable<T> GetQueryable<T>(bool all = true, long date = 0) where T : BaseEntity
        {
            var collection = _database.GetCollection<T>(GetCollectionName<T>());
            var queryable = collection.AsQueryable();
            if (!all)
            {
                queryable = queryable.Where(x => x.Active);
            }

            if (date > 0)
            {
                queryable = queryable.Where(x => x.LastUpdate > date);
            }

            return queryable;
        }

        public T UpdateEntity<T>(T entity) where T : BaseEntity
        {
            var c = _database.GetCollection<T>(GetCollectionName<T>());
            entity.LastUpdate = DateTime.UtcNow.Ticks;
            c.ReplaceOne(x => x.Id == entity.Id, entity);
            return GetEntityById<T>(entity.Id);
        }

        public void DeleteEntity<T>(string entityId, bool force = false) where T : BaseEntity
        {
            var c = _database.GetCollection<T>(GetCollectionName<T>());
            if (force)
            {
                c.DeleteOne(x => x.Id == entityId);
            }
            else
            {
                UpdateField<T, bool>(entityId, x => x.Active, false);
            }
        }

        private IMongoCollection<T> GetCollection<T>() where T : BaseEntity
        {
            return _database.GetCollection<T>(GetCollectionName<T>());
        }

        public void UpdateField<T, TField>(string id, Expression<Func<T, TField>> field, TField value)
            where T : BaseEntity
        {
            var collection = GetCollection<T>();
            var update = Builders<T>.Update
                .Set(field, value)
                .Set(x => x.LastUpdate, DateTime.UtcNow.Ticks);
            collection.UpdateOne(x => x.Id == id, update);
        }

        public void AddItemToList<T, TField>(string entityId, Expression<Func<T, IEnumerable<TField>>> list,
            TField item) where T : BaseEntity
        {
            var collection = GetCollection<T>();
            var updateDefinition = Builders<T>.Update
                .Push(list, item)
                .Set(x => x.LastUpdate, DateTime.UtcNow.Ticks);
            collection.UpdateOne(x => x.Id == entityId, updateDefinition);
        }

        public void DeleteItemFromList<T, TItem>(string entityId, Expression<Func<T, IEnumerable<TItem>>> field,
            Expression<Func<TItem, bool>> filter) where T : BaseEntity
        {
            var collection = GetCollection<T>();
            var updateDefinition = Builders<T>.Update
                .PullFilter(field, filter)
                .Set(x => x.LastUpdate, DateTime.UtcNow.Ticks);
            collection.UpdateOne(x => x.Id == entityId, updateDefinition);
        }

        private static string GetCollectionName<T>()
        {
            return nameof(T);
        }
    }
}