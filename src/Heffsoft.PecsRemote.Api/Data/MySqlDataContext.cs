using Dapper;
using Heffsoft.PecsRemote.Api.Data.Models;
using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Heffsoft.PecsRemote.Api.Data
{
    public class MySqlDataContext : IDataContext
    {
        internal readonly MySqlConnection Connection;
        internal MySqlTransaction Transaction;

        private static Boolean databaseInitialised = false;
        private static Object initialisationLock = new Object();

        public MySqlDataContext(IConfiguration configuration)
        {
            String connectionString = configuration.GetConnectionString("DefaultConnection");
            if (String.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection String 'DefaultConnection' is not defined or is empty.");

            Connection = new MySqlConnection(connectionString);

            lock (initialisationLock)
            {
                if (databaseInitialised == false)
                {
                    InitialiseDatabase();
                    databaseInitialised = true;
                }
            }
        }

        private void InitialiseDatabase()
        {
            String @namespace = "Heffsoft.PecsRemote.Api.Data.Schema";
            Version dbVersion = GetDatabaseVersion();
            Assembly currentAssembly = Assembly.GetExecutingAssembly();

            foreach (String script in FilterVersion(currentAssembly.GetAllSqlScripts(@namespace), dbVersion))
            {
                using (Stream scriptStream = currentAssembly.GetManifestResourceStream($"{@namespace}.{@script}"))
                {
                    using (TextReader reader = new StreamReader(scriptStream))
                    {
                        String content = reader.ReadToEnd();
                        foreach (String query in content.SplitSql())
                        {
                            try
                            {
                                Connection.ExecuteScalar(query);
                            }
                            catch(Exception ex)
                            {
                                Exception err = new Exception("SQL Error", ex);
                                err.Data["Query"] = query;
                                throw err;
                            }
                        }
                    }
                }
            }
        }

        private IEnumerable<String> FilterVersion(IEnumerable<String> scripts, Version dbVersion)
        {
            foreach (String script in scripts)
            {
                String versionString = script.Substring(0, script.Length - 4);
                Version scriptVersion = Version.Parse(versionString);
                if (scriptVersion > dbVersion)
                {
                    yield return script;
                }
            }
        }

        private Version GetDatabaseVersion()
        {
            try
            {
                IDataRepository<Setting> settingsRepo = GetRepository<Setting>();
                Setting setting = settingsRepo.Find("`Key` = @Key", new { Key = "database:version" }).SingleOrDefault();
                if (setting != null)
                {
                    return Version.Parse(setting.Value);
                }
            }
            catch
            {
            }

            return new Version("0.0.0");
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
