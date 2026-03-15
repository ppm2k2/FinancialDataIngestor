    using Dapper;
    using FinancialDataIngestor.DataLayer.Helpers;
    using FinancialDataIngestor.Models.Type;
    using FundAdminRestAPI.Interfaces.DataAccess;
    using FundAdminRestAPI.Models;
    using MySql.Data.MySqlClient;
    using System.Data;
    using System.Text.Json;

    namespace FundAdminRestAPI.DataLayer.Repositories
    {
        public class FundRepository : IFundRepository, IDisposable
        {
            private readonly DbConnectionFactory _dbFactory;
            private IDbConnection? _connection;
            private readonly object _connLock = new();

            public FundRepository(DbConnectionFactory dbFactory)
            {
                _dbFactory = dbFactory;
            }

            // Kept for compatibility (if used by tests/mocks). _dbFactory must be set before use.
            public FundRepository() { }

            // Ensure a synchronous open connection (create once per repository instance)
            private IDbConnection GetOpenConnection()
            {
                if (_connection != null && _connection.State == ConnectionState.Open)
                    return _connection;

                lock (_connLock)
                {
                    if (_connection != null && _connection.State == ConnectionState.Open)
                        return _connection;

                    _connection = _dbFactory.CreateConnection();
                    if (_connection.State != ConnectionState.Open)
                    {
                        ((MySqlConnection)_connection).Open();
                    }
                    return _connection;
                }
            }

            // Ensure an asynchronous open connection (create once per repository instance)
            private async Task<IDbConnection> GetOpenConnectionAsync()
            {
                if (_connection != null && _connection.State == ConnectionState.Open)
                    return _connection;

                // Double-check locking pattern for async (simple approach)
                lock (_connLock)
                {
                    if (_connection != null && _connection.State == ConnectionState.Open)
                        return _connection;
                    // create connection instance synchronously here; opening will be async below
                    _connection = _dbFactory.CreateConnection();
                }

                if (_connection!.State != ConnectionState.Open)
                {
                    await ((MySqlConnection)_connection).OpenAsync();
                }

                return _connection;
            }

            /// <summary>
            /// Following Method determines PL of Crypto Funds based on latest security prices 
            /// </summary>
            /// <returns></returns>
            public ClientAccountDTO? GetFundData()
            {
                var connection = GetOpenConnection();

                // Aggregate clients -> accounts -> holdings
                var clientLookup = new Dictionary<string, ClientAccountDTO>(StringComparer.OrdinalIgnoreCase);

                // Multi-map to ClientAccountDTO, Account, Holding
                var mapped = connection.Query<ClientAccountDTO, AccountDTO, HoldingDTO, ClientAccountDTO>(
                    SqlQueries.SelecFundAdmin,
                    (client, account, holding) =>
                    {
                        // Normalize client id to string key (works for string/Guid/int via ToString)
                        var clientKey = client.ClientId?.ToString() ?? string.Empty;

                        if (!clientLookup.TryGetValue(clientKey, out var existingClient))
                        {
                            existingClient = client;
                            // Ensure Accounts list is initialized
                            if (existingClient.Accounts == null)
                            {
                                existingClient.Accounts = new List<AccountDTO>();
                            }
                            clientLookup.Add(clientKey, existingClient);
                        }

                        if (account != null && (account.AccountId != null && !string.IsNullOrEmpty(account.AccountId.ToString())))
                        {
                            // Find or add account
                            var existingAccount = existingClient.Accounts.FirstOrDefault(a => a.AccountId?.ToString() == account.AccountId?.ToString());
                            if (existingAccount == null)
                            {
                                if (account.Holdings == null)
                                {
                                    account.Holdings = new List<HoldingDTO>();
                                }
                                existingClient.Accounts.Add(account);
                                existingAccount = account;
                            }

                            // Add holding if present (check a key property like Ticker to avoid adding empty rows)
                            if (holding != null && !string.IsNullOrEmpty((holding.Ticker ?? string.Empty)))
                            {
                                if (existingAccount.Holdings == null)
                                {
                                    existingAccount.Holdings = new List<HoldingDTO>();
                                }
                                existingAccount.Holdings.Add(holding);
                            }
                        }

                        return existingClient;
                    },
                    splitOn: "AccountId,Ticker"
                ).ToList();

                // Return the first aggregated client (or null)
                return clientLookup.Values.FirstOrDefault();
            }

            public async Task<bool> InsertFundDataAsync(ClientAccountDTO client)
            {
                var connection = await GetOpenConnectionAsync();

                // 3. Start the transaction (MySqlConnection for async BeginTransactionAsync)
                var mySqlConn = (MySqlConnection)connection;
                using var transaction = await mySqlConn.BeginTransactionAsync();

                try
                {
                    // Execute Dapper queries using the established connection and transaction
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
                            // Ensure the holding insert participates in the same transaction
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

                    await transaction.CommitAsync();
                    return true;
                }
                catch (Exception)
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }

            public void Dispose()
            {
                if (_connection != null)
                {
                    try
                    {
                        if (_connection.State != ConnectionState.Closed)
                        {
                            _connection.Close();
                        }
                    }
                    catch
                    {
                        // ignore
                    }
                    finally
                    {
                        _connection.Dispose();
                        _connection = null;
                    }
                }
            }
        }
    }
