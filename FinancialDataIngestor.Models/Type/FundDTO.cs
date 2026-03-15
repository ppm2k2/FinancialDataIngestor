using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FundAdminRestAPI.Models.Type
{
    public class FundDTO
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public List<SecurityDTO> Securities { get; set; }
        public decimal InitialMV { get; set; }
        public decimal CurrentMV { get; set; }
        public decimal PL { get; set; }
        public decimal PLPercent { get; set; }
    }
}
