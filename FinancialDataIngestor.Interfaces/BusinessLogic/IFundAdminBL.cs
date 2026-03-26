using FinancialDataIngestor.Models.Type;
using FundAdminRestAPI.Models;

namespace FundAdminRestAPI.Interfaces.BusinessLogic
{
    public interface IFundAdminBL
    {

        public Task<RepetedResponse<ClientAccountDTO>> GetFundData();

        public Task<RepetedResponse<object>> InsertFundDataAsync();
        public Task<RepetedResponse<object>> InsertFundDataAsync(string zipUrl);

        //public Task<RepetedResponse<object>> ExecuteFullEtlProcessAsync();


    }
}
