using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Data
{
    public class MySqlDataContext : IDataContext
    {
        internal readonly MySqlConnection Connection;
        internal MySqlTransaction Transaction;

        public MySqlDataContext(IConfiguration configuration)
        {
            String connectionString = configuration.GetConnectionString("DefaultConnection");
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection String 'DefaultConnection' is not defined or is empty.");

            Connection = new MySqlConnection(connectionString);
        }

        public void BeginTransaction()
        {
            if (Transaction != null)
                throw new InvalidOperationException("Transaction is already in progress");

            Transaction = Connection.BeginTransaction();
        }

        public void Commit()
        {
            if (Transaction == null)
                throw new InvalidOperationException("No transaction is currently in progress");

            Transaction.Commit();
            Transaction.Dispose();
            Transaction = null;
        }

        public IDataRepository<T> GetRepository<T>()
        {
            return new MySqlDataRepository<T>(this);
        }

        public void Rollback()
        {
            if (Transaction == null)
                throw new InvalidOperationException("No transaction is currently in progress");

            Transaction.Rollback();
            Transaction.Dispose();
            Transaction = null;
        }
    }
}
