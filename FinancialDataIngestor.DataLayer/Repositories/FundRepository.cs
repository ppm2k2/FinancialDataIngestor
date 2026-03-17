using Dapper;
using FinancialDataIngestor.Models.Type;
using FundAdminRestAPI.Interfaces.DataAccess;
using System.Data;
using System.Collections.Generic;
using System.Linq;
using FinancialDataIngestor.DataLayer.Constants;
using FinancialDataIngestor.DataLayer.DBConnection;

namespace FundAdminRestAPI.DataLayer.Repositories
{
    public class FundRepository : IFundRepository
    {
        private readonly DbConnectionFactory _dbFactory;

        public FundRepository(DbConnectionFactory dbFactory)
        {
            _dbFactory = dbFactory;
        }

        public ClientAccountDTO GetFundData()
        {
            // Dictionary is local to the method, making it thread-safe
            var clientLookup = new Dictionary<string, ClientAccountDTO>(StringComparer.OrdinalIgnoreCase);

            using var connection = _dbFactory.CreateConnection();
            connection.Open();

            connection.Query<ClientAccountDTO, AccountDTO, HoldingDTO, ClientAccountDTO>(
                SqlQueries.SelecFundAdmin,
                (client, account, holding) =>
                {
                    if (!clientLookup.TryGetValue(client.ClientId, out var existingClient))
                    {
                        existingClient = client;
                        existingClient.Accounts = new List<AccountDTO>();
                        clientLookup.Add(existingClient.ClientId, existingClient);
                    }

                    if (account != null && !string.IsNullOrEmpty(account.AccountId))
                    {
                        var existingAccount = existingClient.Accounts.Find(a => a.AccountId == account.AccountId);
                        if (existingAccount == null)
                        {
                            existingAccount = account;
                            existingAccount.Holdings = new List<HoldingDTO>();
                            existingClient.Accounts.Add(existingAccount);
                        }

                        if (holding != null && !string.IsNullOrEmpty(holding.Ticker))
                        {
                            existingAccount.Holdings.Add(holding);
                        }
                    }
                    return existingClient;
                },
                splitOn: "AccountId,Ticker"
            );

            // Use First() to satisfy the non-nullable return type required by IFundRepository.
            // This will throw InvalidOperationException if there are no clients.
            return clientLookup.Values.First();
        }

        public async Task<bool> InsertFundDataAsync(ClientAccountDTO client)
        {
            using var connection = _dbFactory.CreateConnection();
            connection.Open();
            using var transaction = connection.BeginTransaction();

            try
            {
                await connection.ExecuteAsync(SqlQueries.InsertClient, client, transaction);

                foreach (var acc in client.Accounts)
                {
                    await connection.ExecuteAsync(SqlQueries.InsertAccount, new
                    {
                        acc.AccountId,
                        client.ClientId,
                        acc.AccountType,
                        acc.Custodian,
                        acc.OpenedDate,
                        acc.Status,
                        acc.CashBalance,
                        acc.TotalValue
                    }, transaction);

                    foreach (var h in acc.Holdings)
                    {
                        await connection.ExecuteAsync(SqlQueries.InsertHolding, new
                        {
                            acc.AccountId,
                            h.Ticker,
                            h.Cusip,
                            h.Description,
                            h.Quantity,
                            h.MarketValue,
                            h.CostBasis,
                            h.Price,
                            h.AssetClass
                        }, transaction);
                    }
                }

                transaction.Commit();
                return true;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }
    }
}