using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Web;

namespace BadMajor
{
    public static class DatabaseConnection
    {
        static MySqlConnection databaseConnection = null;
        public static MySqlConnection getDBConnection()
        {
            try
            {
                if (databaseConnection == null)
                {
                    string connectionString = ConfigurationManager.ConnectionStrings["MysqlConnection"].ConnectionString;
                    databaseConnection = new MySqlConnection(connectionString);
                }
                return databaseConnection;
            }
            catch
            {
                throw;
            }
            
        }
    }
}