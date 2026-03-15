using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundAdminRestAPI.Models.Type
{
    public class RatesDTO
    {
        public string Currency { get; set; }
        public Dictionary<string, decimal> Rates { get; set; }

    }
}
