using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinancialDataIngestor.Models.Type
{
    public class AccountDTO
    {
        [JsonPropertyName("account_id")] public string AccountId { get; set; }
        [JsonPropertyName("account_type")] public string AccountType { get; set; }
        [JsonPropertyName("custodian")] public string Custodian { get; set; }
        [JsonPropertyName("opened_date")] public string OpenedDate { get; set; }
        [JsonPropertyName("status")] public string Status { get; set; }
        [JsonPropertyName("holdings")] public List<HoldingDTO> Holdings { get; set; }
        [JsonPropertyName("cash_balance")] public decimal CashBalance { get; set; }
        [JsonPropertyName("total_value")] public decimal TotalValue { get; set; }
    }
}
