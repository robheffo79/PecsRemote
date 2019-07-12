using Dapper;
using Heffsoft.PecsRemote.Api.Interfaces;
using System;
using System.Collections.Generic;

namespace Heffsoft.PecsRemote.Api.Data
{
    public class MySqlDataRepository<T> : IDataRepository<T>
    {
        private readonly MySqlDataContext context;
        private readonly String tableName;

        internal MySqlDataRepository(MySqlDataContext dataContext)
        {
            context = dataContext;
            tableName = typeof(T).GetTableName();
        }

        public void Delete(T obj)
        {
            String query = $"DELETE FROM `{tableName}` WHERE `Id` = @Id;";
            context.Connection.Execute(query, obj, context.Transaction);
        }

        public void Delete<U>(U id)
        {
            String query = $"DELETE FROM `{tableName}` WHERE `Id` = @Id;";
            context.Connection.Execute(query, new { Id = id }, context.Transaction);
        }

        public IEnumerable<T> Find(String where, Object args)
        {
            String query = $"SELECT * FROM `{tableName}` WHERE {where};";
            return context.Connection.Query<T>(query, args, context.Transaction);
        }

        public T Get<U>(U id)
        {
            String query = $"SELECT * FROM `{tableName}` WHERE `Id` = @Id;";
            return context.Connection.QuerySingleOrDefault<T>(query, new { Id = id }, context.Transaction);
        }

        public IEnumerable<T> GetAll()
        {
            String query = $"SELECT * FROM `{tableName}`;";
            return context.Connection.Query<T>(query, null, context.Transaction);
        }

        public U Insert<U>(T obj)
        {
            String query = typeof(T).GetInsertQuery<U>();
            return context.Connection.ExecuteScalar<U>(query, obj, context.Transaction);
        }

        public void Update(T obj)
        {
            String query = typeof(T).GetUpdateQuery();
            context.Connection.Execute(query, obj, context.Transaction);
        }
    }
}
