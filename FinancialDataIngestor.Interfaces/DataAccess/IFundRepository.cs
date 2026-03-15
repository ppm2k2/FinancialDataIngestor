using FundAdminRestAPI.Models;
using FundAdminRestAPI.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundAdminRestAPI.Interfaces.DataAccess
{
    public interface IFundRepository
    {
        public List<FundDTO> GetFundPL();


    }
}
