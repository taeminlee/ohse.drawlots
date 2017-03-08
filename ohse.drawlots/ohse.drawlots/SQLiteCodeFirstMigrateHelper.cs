using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Data.Entity;
using System.Data.SqlClient;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ohse.drawlots
{
    internal static class WorkingConn
    {
        internal static string GetConnStr()
        {
            //<add name="WorkingModel" connectionString="data source=D:\test.db;initial catalog=HanRiver.Model.WorkingModel;integrated security=True;password=rhfueo!;MultipleActiveResultSets=True;App=EntityFramework" providerName="System.Data.SQLite.EF6" />
            //To get the location the assembly normally resides on disk or the install directory
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //once you have the path you get the directory with:
            var directory = System.IO.Path.GetDirectoryName(path);

            // Specify the provider name, server and database.
            string providerName = "System.Data.SQLite.EF6";
            string serverName = directory + @"\newdb.db";

            // Initialize the connection string builder for the
            // underlying provider.
            SqlConnectionStringBuilder sqlBuilder =
                new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = serverName;
            sqlBuilder.Password = "tndjq1";
            sqlBuilder.IntegratedSecurity = true;
            sqlBuilder.ApplicationName = "EntityFramework";
            sqlBuilder.MultipleActiveResultSets = true;

            // Build the SqlConnection connection string.
            string providerString = sqlBuilder.ToString();

            return providerString;
        }
    }

    public static class SQLiteCodeFirstMigrateHelper
    {
        /// <summary>
        /// Due to the constraints on SQLite alter table grammar, only supports add column(s). If needs more complex cases (e.g. remove column) needs another implementation.
        /// </summary>
        /// <param name="ctx">Entity code first DbContext</param>
        public static void UpdateModel(DbContext ctx)
        {
            var fields = ctx.GetType().GetRuntimeFields();
            foreach (var field in fields)
            {
                if (field.FieldType.IsGenericType == false) continue;
                try
                {
                    Type tableType = field.FieldType.GenericTypeArguments.First();
                    var DBtypes =
                        ctx.Database.SqlQuery<ColumnInfo>($"pragma table_info(\"{tableType.Name}\");").ToList();
                    var entityTypes = tableType.GetProperties().ToList();

                    if (DBtypes.Count == 0)
                    {
                        ctx.Database.ExecuteSqlCommand(MakeCreateSql(tableType.Name, entityTypes));
                    }
                    else
                    {
                        var DBColNames = DBtypes.Select(info => info.name);

                        foreach (var propertyInfo in entityTypes.Where(info => DBColNames.Contains(info.Name) == false))
                        {
                            ctx.Database.ExecuteSqlCommand(MakeAlterSql(tableType.Name, propertyInfo));
                        }
                    }
                }
                catch
                {
                    ;
                }
                
            }
        }

        private static string MakeCreateSql(string tableTypeName, List<PropertyInfo> entityTypes)
        {
            string primaryKey = "";
            var keyTypes =
                entityTypes.Where(info => info.GetCustomAttributes<KeyAttribute>().Count() > 0)
                    .Select(info => info.Name)
                    .ToList();
            if (keyTypes.Count > 0)
            {
                primaryKey = $"PRIMARY KEY ({string.Join(", ", keyTypes)})";
            }
            return $"CREATE TABLE \"{tableTypeName}\" ({string.Join(", ", entityTypes.Select(info => info.GetColumnDef()))}, {primaryKey});";
        }

        private static string MakeAlterSql(string tableName, PropertyInfo p)
        {
            return $"ALTER TABLE {tableName} ADD COLUMN {p.GetColumnDef()}";
        }

        public static string GetColumnDef(this PropertyInfo p)
        {
            return $"[{p.Name}] {p.GetColumnType()}";
        }

        public static string GetColumnType(this PropertyInfo p)
        {
            var dbColType = p.PropertyType;
            if (dbColType == typeof(string))
            {
                return "nvarchar";
            }
            else if (dbColType == typeof(float) || dbColType == typeof(float?))
            {
                return "float";
            }
            else if (dbColType == typeof(double) || dbColType == typeof(double?))
            {
                return "float";
            }
            else if (dbColType == typeof(short) || dbColType == typeof(short?))
            {
                return "INTEGER";
            }
            else if (dbColType == typeof(int) || dbColType == typeof(int?))
            {
                return "INTEGER";
            }
            else if (dbColType == typeof(long) || dbColType == typeof(long?))
            {
                return "INTEGER";
            }
            else if (dbColType == typeof(bool))
            {
                return "bit";
            }
            return "";
        }

        public class ColumnInfo
        {
            public int cid { get; set; }
            public string name { get; set; }
            public string type { get; set; }
            public bool notnull { get; set; }
            public object dflt_value { get; set; }
            public bool pk { get; set; }
        }
    }
}
