using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundAdminRestAPI.Models.Type
{
    public class SecurityDTO
    {
        public string Name { get; set; }
        public decimal Quantity { get; set; }
        public decimal AvgPrice { get; set; }
    }
}
