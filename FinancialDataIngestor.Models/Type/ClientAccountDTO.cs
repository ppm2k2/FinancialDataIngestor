using System;
using System.Collections.Generic;
using System.Security.Principal;
using System.Text;
using System.Text.Json.Serialization;

namespace FinancialDataIngestor.Models.Type
{
    public class ClientAccountDTO
    {
        [JsonPropertyName("client_id")] 
        public string ClientId { get; set; }

        [JsonPropertyName("first_name")] 
        public string FirstName { get; set; }
        [JsonPropertyName("last_name")] 
        public string LastName { get; set; }
        [JsonPropertyName("email")] 
        public string Email { get; set; }
        [JsonPropertyName("accounts")] 
        public List<AccountDTO> Accounts { get; set; }
        [JsonPropertyName("advisor_id")] 
        public string AdvisorId { get; set; }
        [JsonPropertyName("last_updated")] 
        public DateTime LastUpdated { get; set; }
    }
}
