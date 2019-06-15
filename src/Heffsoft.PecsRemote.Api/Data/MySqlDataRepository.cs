﻿using Dapper;
using Heffsoft.PecsRemote.Api.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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

        public void Delete(Int32 id)
        {
            String query = $"DELETE FROM `{tableName}` WHERE `Id` = @Id;";
            context.Connection.Execute(query, new { Id = id }, context.Transaction);
        }

        public IEnumerable<T> Find(String where, Object args)
        {
            String query = $"SELECT * FROM `{tableName}` WHERE {where};";
            return context.Connection.Query<T>(query, args, context.Transaction);
        }

        public T Get(Int32 id)
        {
            String query = $"SELECT * FROM `{tableName}` WHERE `Id` = @Id;";
            return context.Connection.QuerySingleOrDefault<T>(query, new { Id = id }, context.Transaction);
        }

        public IEnumerable<T> GetAll()
        {
            String query = $"SELECT * FROM `{tableName}`;";
            return context.Connection.Query<T>(query, null, context.Transaction);
        }

        public Int32 Insert(T obj)
        {
            String query = typeof(T).GetInsertQuery();
            return context.Connection.ExecuteScalar<Int32>(query, obj, context.Transaction);
        }

        public void Update(T obj)
        {
            String query = typeof(T).GetUpdateQuery();
            context.Connection.Execute(query, obj, context.Transaction);
        }
    }
}
