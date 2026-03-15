using FundAdminRestAPI.Interfaces.DataAccess;
using FundAdminRestAPI.Models;
using FundAdminRestAPI.Models.Type;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using FundAdminRestAPI.Common.Extentions;
using FundAdminRestAPI.Interfaces.BusinessLogic;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using FundAdminRestAPI.DataLayer.Services;

namespace FundAdminRestAPI.DataLayer.Repositories
{
    public class FundRepository : IFundRepository
    {
        public FundRepository() { }

        /// <summary>
        /// Following Method determines PL of Crypto Funds based on latest security prices 
        /// </summary>
        /// <returns></returns>
        public List<FundDTO> GetFundPL()
        {
            decimal pl = 0;
            decimal initialMV = 0;
            decimal currentMV = 0;
            List<FundDTO> Portfolios = new();
            var result = new RepetedResponse<FundListDTO>();
            List<FundListDTO> fundListResponse = new List<FundListDTO>();

            // Get Current Rate from Conbase API.
            PricingDTO currencyRates = FundAdminService.GetRates<PricingDTO>(Constants.JSONRATESURL);
            RatesDTO currencyRatesDTO = currencyRates.data;
            Dictionary<string, decimal> rates = currencyRatesDTO.Rates;
            
            // Get JSON Data from portfolio.json and calculate fund's initial market value and
            // P/L with respect to latest currency rates.
            StreamReader reader = new(Constants.JSONDATA);
            var json = reader.ReadToEnd();
            var jarray = JArray.Parse(json);
            
            Console.WriteLine(string.Format("********************Crypto Portfolio : {0} *****************", DateTime.Now.ToString("h:mm:ss tt")));
            foreach (var item in jarray)
            {
                FundDTO portfolio = item.ToObject<FundDTO>();
                foreach (var secirity in portfolio.Securities)
                {
                    // Beginning of Portfolio Market Value..
                    initialMV += secirity.Quantity * secirity.AvgPrice;

                    switch (secirity.Name)
                    {
                        // (Quantity * (CurrentPrice - AvgPrice))
                        case "BTC":
                            currentMV += secirity.Quantity * (1 / rates[Constants.BTC_TICKER]); // Current BTC MV
                            pl += secirity.Quantity * (1 / rates[Constants.BTC_TICKER] - secirity.AvgPrice); // Current BTC PL
                            break;
                        case "ETH":
                            currentMV += secirity.Quantity * (1 / rates[Constants.ETH_TICKER]); // Current ETH MV
                            pl += secirity.Quantity * (1 / rates[Constants.ETH_TICKER] - secirity.AvgPrice);
                            break;
                        case "AAVE":
                            currentMV += secirity.Quantity * (1 / rates[Constants.AAVE_TICKER]); // Current AAVE MV
                            pl += secirity.Quantity * (1 / rates[Constants.AAVE_TICKER] - secirity.AvgPrice);
                            break;
                        case "SOL":
                            currentMV += secirity.Quantity * (1 / rates[Constants.SOL_TICKER]); // Current SOL MV
                            pl += secirity.Quantity * (1 / rates[Constants.SOL_TICKER] - secirity.AvgPrice);
                            break;
                    }
                }
                
                // Beginning Portfolio MV
                portfolio.InitialMV = initialMV;
                portfolio.CurrentMV = currentMV;
                portfolio.PL = pl;
                portfolio.PLPercent = (currentMV - initialMV) / initialMV * 100;

                LogFundData(portfolio);

                Portfolios.Add(portfolio);
            }
            return Portfolios;
        }

        /// <summary>
        /// Logs Fund Data in Console
        /// </summary>
        /// <param name="portfolio"></param>
        private static void LogFundData(FundDTO portfolio)
        {
            
            Console.WriteLine(string.Format(" {0} Initial Market Value : ${1}, Current Market Value : ${2}, P/L : ${3}, P/L %: {4}%",
                portfolio.Name, Math.Round(portfolio.InitialMV, 8), Math.Round(portfolio.CurrentMV, 8), Math.Round(portfolio.PL, 8), Math.Round(portfolio.PLPercent, 8)));

        }
    }
}
