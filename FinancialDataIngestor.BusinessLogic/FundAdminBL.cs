using FinancialDataIngestor.DataLayer.Services;
using FinancialDataIngestor.Interfaces.DataAccess;
using FinancialDataIngestor.Models.Type;
using FundAdminRestAPI.Interfaces.BusinessLogic;
using FundAdminRestAPI.Interfaces.DataAccess;
using FundAdminRestAPI.Models;
using MySqlX.XDevAPI;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

public class FundAdminBL : IFundAdminBL
{
    private readonly IFundRepository _fundRepository;
    private readonly IAuditService _auditService;

    public FundAdminBL(IFundRepository fundRepository)
    {
        _fundRepository = fundRepository;
    }

    // Use Constructor Injection instead of 'new'
    public FundAdminBL(IFundRepository fundRepository, IAuditService auditService)
    {
        _fundRepository = fundRepository;
        _auditService = auditService;
    }

    public async Task<RepetedResponse<ClientAccountDTO>> GetFundData()
    {
        var result = new RepetedResponse<ClientAccountDTO>();
        // Ensure ServiceReponse is initialized before use
        result.ServiceReponse = new Response();

        try
        {
            // Call the synchronous repository method that exists on IFundRepository.
            var fundResponse = _fundRepository.GetFundData();

            // Wrap the single DTO into a list to match RepetedResponse.Result type.
            result.Result = new List<ClientAccountDTO> { fundResponse };
            result.ServiceReponse.IsSuccessful = true;
            result.ServiceReponse.Message = new List<string> { "Fund data retrieved successfully." };
        }
        catch (Exception ex)
        {
            // Always handle potential repository failures
            result.ServiceReponse.IsSuccessful = false;
            result.ServiceReponse.ErrorMessage = ex.Message;
            result.ServiceReponse.Message = new List<string> { "Failed to retrieve fund data." };
        }

        return await Task.FromResult(result);
    }

    public async Task<RepetedResponse<object>> InsertFundDataAsync()
    {
        var result = new RepetedResponse<object>();
        result.ServiceReponse = new Response();

        // Variables declared at method scope so finally block can access them
        ClientAccountDTO client = null;
        bool isSuccess = false;
        string errorMessage = null;

        try
        {
            // Call the synchronous repository method that exists on IFundRepository.
            client = _fundRepository.GetFundData();

            // Wrap the single DTO into a list to match RepetedResponse.Result type.
            if (client == null || string.IsNullOrEmpty(client.ClientId))
                throw new ArgumentException("Invalid client data provided.");

            isSuccess = await _fundRepository.InsertFundDataAsync(client);

            // Directly store the bool inside an object list.
            result.Result = new List<object> { isSuccess };

            result.ServiceReponse.IsSuccessful = isSuccess;
            result.ServiceReponse.Message = new List<string>
            {
                isSuccess ? "Data inserted successfully." : "Data insertion failed."
            };
        }
        catch (Exception ex)
        {
            isSuccess = false;
            errorMessage = ex.Message;

            result.ServiceReponse.IsSuccessful = false;
            result.ServiceReponse.ErrorMessage = ex.Message;
            result.ServiceReponse.Message = new List<string> { ex.Message };
        }
        finally
        {            
            await _auditService.LogChangeAsync(
                entityName: "Client",
                entityId: client?.ClientId ?? string.Empty,
                action: "INSERT",
                oldValues: null,
                newValues: new
                {
                    Status = isSuccess ? "SUCCESS" : "FAILURE",
                    Details = errorMessage ?? (isSuccess ? "Operation completed successfully" : "Operation failed")
                }
            );
            
        }

        return result;
    }

}

