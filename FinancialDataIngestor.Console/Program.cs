using FundAdminRestAPI.BusinessLogic;
using FundAdminRestAPI.Models;
using FundAdminRestAPI.Models.Type;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Net;

namespace FundAdmin
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

            do
            {
                FundAdminBL fileAdminBL = new FundAdminBL();
                _ = fileAdminBL.GetFundPL();
            } while (await timer.WaitForNextTickAsync());
        }
    }
}