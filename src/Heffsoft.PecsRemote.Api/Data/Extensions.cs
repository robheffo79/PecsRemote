using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Heffsoft.PecsRemote.Api.Data
{
    public static class Extensions
    {
        public static String GetTableName(this Type type)
        {
            TableAttribute tableAttribute = type.GetCustomAttribute<TableAttribute>();
            if (tableAttribute != null)
                return tableAttribute.Name;

            return type.Name;
        }

        public static IEnumerable<String> GetTableColumnNames(this Type type)
        {
            foreach (PropertyInfo property in type.GetProperties())
            {
                ColumnAttribute columnAttribute = property.GetCustomAttribute<ColumnAttribute>();
                if (columnAttribute != null && String.IsNullOrWhiteSpace(columnAttribute.Name) == false)
                {
                    yield return columnAttribute.Name;
                }
                else
                {
                    yield return property.Name;
                }
            }
        }

        public static IEnumerable<String> GetPropertyNames(this Type type)
        {
            foreach (PropertyInfo property in type.GetProperties())
            {
                yield return property.Name;
            }
        }

        public static String GetInsertQuery<U>(this Type type)
        {
            String tableName = type.GetTableName();
            String[] columnNames = null;
            String[] propertyNames = null;

            if (typeof(U) == typeof(Guid))
            {
                columnNames = type.GetTableColumnNames().ToArray();
                propertyNames = type.GetPropertyNames().ToArray();
            }
            else
            {
                columnNames = type.GetTableColumnNames().Where(n => n != "Id").ToArray();
                propertyNames = type.GetPropertyNames().Where(n => n != "Id").ToArray();
            }

            return $"INSERT INTO `{tableName}` ({String.Join(", ", columnNames.Select(c => $"`{c}`"))}) VALUES({String.Join(", ", propertyNames.Select(p => $"@{p}"))}); SELECT LAST_INSERT_ID();";
        }

        public static String GetUpdateQuery(this Type type)
        {
            String tableName = type.GetTableName();
            String[] columnNames = type.GetTableColumnNames().Where(n => n != "Id").ToArray();
            String[] propertyNames = type.GetPropertyNames().Where(n => n != "Id").ToArray();
            String[] args = new String[columnNames.Length];

            for (Int32 i = 0; i < columnNames.Length; i++)
            {
                args[i] = $"`{columnNames[i]}` = @{propertyNames[i]}";
            }

            return $"UPDATE `{tableName}` SET {String.Join(", ", args)} WHERE `Id` = @Id;";
        }

        public static IServiceCollection AddDataContext(this IServiceCollection services)
        {
            return services.AddTransient<IDataContext, MySqlDataContext>();
        }

        public static IEnumerable<String> GetAllSqlScripts(this Assembly assembly, String @namespace)
        {
            return assembly.GetManifestResourceNames()
                           .Where(r => r.StartsWith(@namespace) && r.EndsWith(".sql"))
                           .Select(r => r.Substring(@namespace.Length + 1))
                           .OrderBy(r => r);
        }

        private static Char[] quotes = { '`', '\'', '"' };
        public static IEnumerable<String> SplitSql(this String sql)
        {
            StringBuilder query = new StringBuilder();

            Char? currentQuote = null;
            foreach(Char c in sql)
            {
                if(quotes.Contains(c))
                {
                    if (currentQuote == null)
                    {
                        currentQuote = c;
                        query.Append(c);
                    }
                    else if(currentQuote.Value == c)
                    {
                        currentQuote = null;
                        query.Append(c);
                    }
                }
                else if(c == ';' && currentQuote == null)
                {
                    query.Append(c);
                    yield return query.ToString().Trim();
                    query = new StringBuilder();
                }
                else
                {
                    query.Append(c);
                }
            }

            String leftover = query.ToString().Trim();
            if(String.IsNullOrWhiteSpace(leftover) == false)
            {
                yield return leftover;
            }
        }
    }
}
