using MySql.Data.MySqlClient;
using System.Data;
using Microsoft.Extensions.Configuration;

namespace FinancialDataIngestor.DataLayer.Helpers
{
    public class DbConnectionFactory
    {
        private readonly IConfiguration _configuration;

        public DbConnectionFactory(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        // Returns an open connection ready to be used by Dapper
        public IDbConnection CreateConnection()
        {
            string connectionString = _configuration.GetConnectionString("Dev_Mysql_Connection");

            // Add this safety check
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string 'Dev_Mysql_Connection' not found in configuration!");
            }

            return new MySqlConnection(connectionString);
        }
    }
}
