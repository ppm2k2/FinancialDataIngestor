using Microsoft.AspNetCore.Mvc;
using FundAdminRestAPI.Interfaces.BusinessLogic;


namespace FundAdminRestAPI.EndPoints
{
    public static class FundService
    {
        public static void MapFundServiceEndpoints(this WebApplication app)
        {
            // POST: Ingest new data (Trigger Database Save)
            app.MapPost("/FundService/CreateFundData", async ([FromServices] IFundAdminBL fundBL) =>
            {
                try
                {
                    // Assuming ProcessAndSaveData returns a boolean or status object
                    await fundBL.InsertFundDataAsync();
                    return Results.Accepted("/FundService/CreateFundData", "Ingestion process completed successfully.");
                }
                catch (Exception ex)
                {
                    // Return a 500 error if something goes wrong in the Data/BL layer
                    return Results.Problem(detail: ex.Message, title: "Ingestion Failed", statusCode: 500);
                }
            });

            app.MapGet("/FundService/GetFundData", async ([FromServices] IFundAdminBL fundBL) =>
              {
                  var data = await fundBL.GetFundData();

                  // Better practice: Return an explicit IResult
                  return data != null ? Results.Ok(data) : Results.NotFound("No fund data found.");
              });


        }
    }
}
