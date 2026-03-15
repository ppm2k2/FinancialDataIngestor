using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialDataIngestor.Interfaces.DataAccess
{
    public interface IAuditService
    {
        Task LogChangeAsync(string entityName, string entityId, string action, object oldValues, object newValues);
    }
}
