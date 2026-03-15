using System;
using System.Collections.Generic;
using System.Text;

namespace FinancialDataIngestor.DataLayer.Helpers
{
    public static class SqlQueries
    {
        public const string InsertClient = @"
            INSERT INTO Clients (client_id, first_name, last_name, email, advisor_id, last_updated) 
            VALUES (@ClientId, @FirstName, @LastName, @Email, @AdvisorId, @LastUpdated)";

        public const string InsertAccount = @"
            INSERT INTO Accounts (account_id, client_id, account_type, custodian, opened_date, status, cash_balance, total_value) 
            VALUES (@AccountId, @ClientId, @AccountType, @Custodian, @OpenedDate, @Status, @CashBalance, @TotalValue)";

        public const string InsertHolding = @"
            INSERT INTO Holdings (account_id, ticker, cusip, description, quantity, market_value, cost_basis, price, asset_class) 
            VALUES (@AccountId, @Ticker, @Cusip, @Description, @Quantity, @MarketValue, @CostBasis, @Price, @AssetClass)";


        public const string SelecFundAdmin = @"
            SELECT 
                c.client_id AS ClientId,
                CONCAT(c.first_name, ' ', c.last_name) AS ClientName,
                a.account_id AS AccountId,
                a.account_type AS AccountType,
                a.custodian AS Custodian,
                a.opened_date AS OpenedDate,
                a.status AS Status,
                a.cash_balance AS CashBalance,
                a.total_value AS TotalValue,
                h.ticker AS Ticker,
                h.cusip AS Cusip,
                h.description AS HoldingDescription,
                h.quantity AS Quantity,
                h.market_value AS MarketValue,
                h.cost_basis AS CostBasis,
                h.price AS Price,
                h.asset_class AS AssetClass
            FROM Clients c
            LEFT JOIN Accounts a ON a.client_id = c.client_id
            LEFT JOIN Holdings h ON h.account_id = a.account_id
            ORDER BY c.client_id;
        ";
        public const string InsertAuditLog = @"INSERT INTO AuditLog (entity_name, entity_id, action_type, old_values, new_values) 
                             VALUES (@entityName, @entityId, @action, @oldValues, @newValues)";
    }
}
