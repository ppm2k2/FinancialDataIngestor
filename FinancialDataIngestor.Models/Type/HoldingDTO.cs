using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json.Serialization;

namespace FinancialDataIngestor.Models.Type
{
    public class HoldingDTO
    {
        [JsonPropertyName("ticker")] public string Ticker { get; set; }
        [JsonPropertyName("cusip")] public string Cusip { get; set; }
        [JsonPropertyName("description")] public string Description { get; set; }
        [JsonPropertyName("quantity")] public decimal Quantity { get; set; }
        [JsonPropertyName("market_value")] public decimal MarketValue { get; set; }
        [JsonPropertyName("cost_basis")] public decimal CostBasis { get; set; }
        [JsonPropertyName("price")] public decimal Price { get; set; }
        [JsonPropertyName("asset_class")] public string AssetClass { get; set; }
    }
}
