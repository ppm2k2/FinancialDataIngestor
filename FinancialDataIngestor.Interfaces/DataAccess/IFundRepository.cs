using FinancialDataIngestor.Models.Type;

namespace FundAdminRestAPI.Interfaces.DataAccess
{
    public interface IFundRepository
    {
        public ClientAccountDTO GetFundData();

        public Task<bool> InsertFundDataAsync(ClientAccountDTO client);

    }
}
