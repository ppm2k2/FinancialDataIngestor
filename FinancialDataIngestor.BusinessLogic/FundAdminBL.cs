using FundAdminRestAPI.Common.Extentions;
using FundAdminRestAPI.DataLayer.Repositories;
using FundAdminRestAPI.Interfaces.BusinessLogic;
using FundAdminRestAPI.Interfaces.DataAccess;
using FundAdminRestAPI.Models;
using FundAdminRestAPI.Models.Type;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;



namespace FundAdminRestAPI.BusinessLogic
{
    public class FundAdminBL : IFundAdminBL
    {

        private readonly IFundRepository _fundRepository;

        public FundAdminBL()
        {
        }

        public FundAdminBL(IFundRepository fundRepository) 
        {
           this._fundRepository = fundRepository;
        }

        public async Task<RepetedResponse<FundDTO>> GetFundPL()
        {
            var result = new RepetedResponse<FundDTO>();
            FundRepository _fundRepo = new FundRepository();
            List <FundDTO> fundResponse = new List<FundDTO>();
            if (_fundRepository != null)
                fundResponse = _fundRepository.GetFundPL();
            else
                fundResponse = _fundRepo.GetFundPL();

            result.Result = fundResponse;
            result.ServiceReponse.IsSuccessful = true;
            return result;
        }
    }
}
