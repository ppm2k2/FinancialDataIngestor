using Dapper;
using FinancialDataIngestor.DataLayer.Helpers;
using FinancialDataIngestor.Interfaces.DataAccess;
using System.Text.Json;

namespace FinancialDataIngestor.DataLayer.Services
{
    public class AuditService : IAuditService
    {
        private readonly DbConnectionFactory _dbFactory;

        public AuditService(DbConnectionFactory dbFactory) => _dbFactory = dbFactory;

        public async Task LogChangeAsync(string entityName, string entityId, string action, object oldValues, object newValues)
        {
            using var connection = _dbFactory.CreateConnection();
            
            await connection.ExecuteAsync(SqlQueries.InsertAuditLog, new
            {
                entityName,
                entityId,
                action,
                oldValues = JsonSerializer.Serialize(oldValues),
                newValues = JsonSerializer.Serialize(newValues)
            });
        }

    }
}
