using Heffsoft.PecsRemote.Api.Interfaces;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Reflection;
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

            for(Int32 i = 0; i < columnNames.Length; i++)
            {
                args[i] = $"`{columnNames[i]}` = @{propertyNames[i]}";
            }

            return $"UPDATE `{tableName}` SET {String.Join(", ", args)} WHERE `Id` = @Id;";
        }

        public static IServiceCollection AddDataContext(this IServiceCollection services)
        {
            return services.AddTransient<IDataContext, MySqlDataContext>();
        }
    }
}
