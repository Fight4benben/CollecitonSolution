using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CollectionData.Util
{
    public class DapperConfig
    {
        private static string DefaultMySqlConnectionString = @"Server=127.0.0.1;port=3306;Database=EnergyDB;userid=root;password=root";

        public static IDbConnection GetSqlConnection(string sqlConnectionString)
        {
            if (string.IsNullOrWhiteSpace(sqlConnectionString))
                sqlConnectionString = DefaultMySqlConnectionString;

            IDbConnection conn = new MySqlConnection(sqlConnectionString);
            conn.Open();

            return conn;
        }
    }
}
