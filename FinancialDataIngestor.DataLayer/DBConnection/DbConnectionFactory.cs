using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Data;
using System.Text;

namespace FinancialDataIngestor.DataLayer.DBConnection
{
    public class DbConnectionFactory
    {
        private readonly string _connectionString;

        public DbConnectionFactory(IConfiguration config)
        {
            _connectionString = config.GetConnectionString("Dev_Mysql_Connection");
        }

        // This is the only place that knows about MySQL
        public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
    }
}
