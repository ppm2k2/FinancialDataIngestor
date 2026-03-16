using FinancialDataIngestor.DataLayer.Services;
using FinancialDataIngestor.Interfaces.DataAccess;
using FinancialDataIngestor.Models;
using FinancialDataIngestor.Models.Type;
using FundAdminRestAPI.Interfaces.BusinessLogic;
using FundAdminRestAPI.Interfaces.DataAccess;
using FundAdminRestAPI.Models;
using MySqlX.XDevAPI;
using MySqlX.XDevAPI.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Security.Principal;
using System.Text.Json;
using System.Threading.Tasks;

public class FundAdminBL : IFundAdminBL
{
    private readonly IFundRepository _fundRepository;
    private readonly IAuditService _auditService;
    private readonly bool isSuccess = false;
    private readonly string errorMessage = null;

    // Shared HttpClient for reuse
    private static readonly HttpClient _httpClient;

    static FundAdminBL()
    {
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", Constants.UserAgent);
    }

    public FundAdminBL(IFundRepository fundRepository)
    {
        _fundRepository = fundRepository;
    }

    public FundAdminBL(IFundRepository fundRepository, IAuditService auditService)
    {
        _fundRepository = fundRepository ?? throw new ArgumentNullException(nameof(fundRepository));
        _auditService = auditService ?? throw new ArgumentNullException(nameof(auditService));
    }
    public async Task<RepetedResponse<ClientAccountDTO>> GetFundData()
    {
        var result = new RepetedResponse<ClientAccountDTO>();
        // Ensure ServiceReponse is initialized before use
        result.ServiceReponse = new Response();

        ClientAccountDTO client = null;
        

        try
        {
            // Call the synchronous repository method that exists on IFundRepository.
            var fundResponse = _fundRepository.GetFundData();
            client = fundResponse;

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
        await Download_File();

        var result = new RepetedResponse<object>();
        result.ServiceReponse = new Response();

        // Variables declared at method scope so finally block can access them
        ClientAccountDTO client = null;
        bool isSuccess = false;
        string errorMessage = null;

        try
        {
            // Call the synchronous repository method that exists on IFundRepository.
            client = ReadFundData();

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
                entityName: "PositionInsertion",
                entityId: client?.ClientId ?? string.Empty,
                action: "INSERT",
                oldValues: null,
                newValues: new
                {
                    Status = isSuccess ? "SUCCESS" : "FAILURE",
                    Details = errorMessage ?? (isSuccess ? "Data insertion Operation completed successfully" : "Operation failed")
                }
            );

        }

        return result;
    }

    private async Task Download_File()
    {
        bool isSuccess = true;
        string errorMessage = null;
        var result = new RepetedResponse<object>();
        result.ServiceReponse = new Response();
        // Use centralized configuration from AppConstants
        string zipUrl = Constants.ZipUrl;
        string timestamp = DateTime.Now.ToString(Constants.TimestampFormat);

        string baseFolder = Constants.BaseFolder;
        string zipFileName = string.Format(Constants.ZipFileNameFormat, timestamp);
        string jsonFileName = string.Format(Constants.JsonFileNameFormat, timestamp);

        string finalZipPath = Path.Combine(baseFolder, zipFileName);
        string finalJsonPath = Path.Combine(baseFolder, jsonFileName);

        try
        {
            if (!Directory.Exists(baseFolder))
                Directory.CreateDirectory(baseFolder);

            Console.WriteLine($"Downloading ZIP to: {finalZipPath}");
            byte[] fileBytes = await _httpClient.GetByteArrayAsync(zipUrl);
            await File.WriteAllBytesAsync(finalZipPath, fileBytes);

            using (ZipArchive archive = ZipFile.OpenRead(finalZipPath))
            {
                var entry = archive.Entries.FirstOrDefault(e => !string.IsNullOrEmpty(e.Name));

                if (entry != null)
                {
                    entry.ExtractToFile(finalJsonPath, overwrite: true);
                    Console.WriteLine($"Extracted and renamed to: {finalJsonPath}");
                    // Directly store the bool inside an object list.
                    result.Result = new List<object> { isSuccess };

                    result.ServiceReponse.IsSuccessful = isSuccess;
                    result.ServiceReponse.Message = new List<string>
                    {
                        isSuccess ? "Data Extracted successfully." : "Data insertion failed."
                    };
                }
                else
                {
                    Console.WriteLine("Error: The ZIP file appears to be empty.");
                }
            }
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
            // 1. Remove the extension to avoid including ".zip" in your parsed data
            string nameWithoutExtension = Path.GetFileNameWithoutExtension(zipFileName);

            // 2. Split the string by the underscore
            string[] parts = nameWithoutExtension.Split('_');

            // 3. Map the parts to your variables
            // parts[0] = "CLT", parts[1] = "29481"
            string clientId = $"{parts[0]}_{parts[1]}";
            string lastName = parts[2];
            string accountId = $"{parts[3]}_{parts[4]}";

            await _auditService.LogChangeAsync(
                entityName: "PositionExtraction",
                entityId: clientId,
                action: "EXTRACT",
                oldValues: null,
                newValues: new
                {
                    Status = isSuccess ? "SUCCESS" : "FAILURE",
                    FileName = zipFileName,
                    AccountId = accountId,
                    Details = errorMessage ?? (isSuccess ? "Data download Operation completed successfully" : "Operation failed")
                }
            );
            

        }
    }

    public ClientAccountDTO ReadFundData()
    {
        string baseFolder = Constants.BaseFolder;

        // 1. Find the latest ZIP file in the directory
        // This assumes the timestamp in the filename allows for alphabetical sorting (e.g., yyyyMMddHHmmss)
        var latestZip = Directory.GetFiles(baseFolder, "*.zip")
                                 .OrderByDescending(f => f)
                                 .FirstOrDefault();

        if (latestZip == null)
        {
            throw new FileNotFoundException("No fund data ZIP files found in the base folder.");
        }

        try
        {
            using (ZipArchive archive = ZipFile.OpenRead(latestZip))
            {
                // 2. Look for the JSON file inside the ZIP
                // We search for the first entry ending in .json
                var jsonEntry = archive.Entries.FirstOrDefault(e => e.FullName.EndsWith(".json", StringComparison.OrdinalIgnoreCase));

                if (jsonEntry == null)
                {
                    throw new FileNotFoundException($"No JSON file found inside the zip: {latestZip}");
                }

                // 3. Open the entry and deserialize
                using (Stream entryStream = jsonEntry.Open())
                {
                    var options = new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    };

                    // Change this line in your ReadFundData method:
                    var data = JsonSerializer.Deserialize<List<ClientAccountDTO>>(entryStream, options);

                    // Then return the first one if that's what you need:
                    return data?.FirstOrDefault();
                }
            }
        }
        catch (Exception ex)
        {
            // Log failure here if needed
            throw new Exception($"Error reading fund data from {latestZip}: {ex.Message}", ex);
        }
    }
}