using FundAdminRestAPI.Models;
using FundAdminRestAPI.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundAdminRestAPI.Interfaces.BusinessLogic
{
    public interface IFundAdminBL
    {
        public Task<RepetedResponse<FundDTO>> GetFundPL();
    }
}
