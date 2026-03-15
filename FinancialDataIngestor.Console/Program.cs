using FundAdminRestAPI.Interfaces.DataAccess;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace FundAdmin
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            var timer = new PeriodicTimer(TimeSpan.FromSeconds(60));

            do
            {
                FundAdminBL fileAdminBL = new FundAdminBL((IFundRepository)null);

                _ = await fileAdminBL.GetFundData();
            } while (await timer.WaitForNextTickAsync());
        }
    }
}