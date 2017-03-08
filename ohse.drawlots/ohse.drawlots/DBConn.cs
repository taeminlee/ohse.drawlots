using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Core.EntityClient;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Windows;

namespace ohse.drawlots
{
    public partial class databaseEntities : DbContext
    {
        public databaseEntities(string connectionStr)
            : base(connectionStr)
        {
        }
    }

    internal static class DBConn
    {
        internal static string GetMigrationPath()
        {
            //To get the location the assembly normally resides on disk or the install directory
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //once you have the path you get the directory with:
            var directory = System.IO.Path.GetDirectoryName(path);

            // Specify the provider name, server and database.
            string serverName = directory + @"\migration.db";

            return serverName;
        }

        internal static string GetPath()
        {
            //To get the location the assembly normally resides on disk or the install directory
            string path = System.Reflection.Assembly.GetExecutingAssembly().Location;

            //once you have the path you get the directory with:
            var directory = System.IO.Path.GetDirectoryName(path);

            // Specify the provider name, server and database.
            string serverName = directory + @"\database.db";

            return serverName;
        }

        internal static string GetConnStr(string serverName)
        {
            // Specify the provider name, server and database.
            string providerName = "System.Data.SQLite.EF6";
            //string databaseName = "hanriver";

            // Initialize the connection string builder for the
            // underlying provider.
            SqlConnectionStringBuilder sqlBuilder =
                new SqlConnectionStringBuilder();

            // Set the properties for the data source.
            sqlBuilder.DataSource = serverName;
            sqlBuilder.Password = "tndjq1";
            sqlBuilder.IntegratedSecurity = true;

            // Build the SqlConnection connection string.
            string providerString = sqlBuilder.ToString();

            // Initialize the EntityConnectionStringBuilder.
            EntityConnectionStringBuilder entityBuilder =
                new EntityConnectionStringBuilder();

            //Set the provider name.
            entityBuilder.Provider = providerName;

            // Set the provider-specific connection string.
            entityBuilder.ProviderConnectionString = providerString;

            // Set the Metadata location.
            entityBuilder.Metadata = @"res://*/DB.csdl|
                            res://*/DB.ssdl|
                            res://*/DB.msl";
            Console.WriteLine(entityBuilder.ToString());

            //<add name="hanriverEntities" connectionString="metadata=res://*/HanModel.csdl|res://*/HanModel.ssdl|res://*/HanModel.msl;provider=System.Data.SQLite.EF6;provider connection string='data source=&quot;C:\Dropbox\2016_1\환경부 과제\31. 한강유역환경청 RISK MAP project\CARISOne\CARISOne\hanriver.db&quot;;password=rhfueo!'" providerName="System.Data.EntityClient" />

            try
            {
                using (EntityConnection conn =
                    new EntityConnection(entityBuilder.ToString()))
                {
                    conn.Open();
                    Console.WriteLine("Just testing the connection.");
                    conn.Close();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
            return entityBuilder.ToString();
        }
    }
}