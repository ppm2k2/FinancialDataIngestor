using FundAdminRestAPI.Interfaces.BusinessLogic;
using Microsoft.AspNetCore.Mvc;

namespace FundAdminRestAPI.EndPoints
{
    public static class FundService
    {
        public static void MapFundServiceEndpoints(this WebApplication app)
        {
            app.MapGet("/FundService/GetFundPL", async ([FromServices] IFundAdminBL FundBL) =>
            {
                return await FundBL.GetFundPL();
            });
        }
    }
}
