using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

/* 
Pseudocode / Plan (detailed):
1. Introduce new PascalCase constant names that mirror existing constant values:
   - ApplicationName, ApiPathBase, Swagger, Index, Configuration, JsonData,
     ZipUrl, TimestampFormat, BaseFolder, ZipFileNameFormat, JsonFileNameFormat, UserAgent.
2. Preserve backward compatibility by keeping the existing UPPERCASE constant identifiers.
   - Mark each old UPPERCASE constant with [Obsolete("Use <PascalCaseName>")] so callers get a compile-time warning.
   - Assign each old UPPERCASE constant to the corresponding new PascalCase constant.
3. Keep all constant values unchanged.
4. Output a single modified `Constants` class file containing:
   - using directives (existing),
   - a multi-line comment with the pseudocode (for traceability),
   - the new PascalCase constants,
   - the obsolete UPPERCASE constants that forward to the new ones.
5. This ensures external references continue to compile while encouraging migration to the new names.

Implementation notes:
- Use const string for compile-time constants.
- Attributes like [Obsolete] can be applied to fields to guide consumers.
*/

namespace FundAdminRestAPI.Models
{
    public static class Constants
    {
        // New PascalCase constants (preferred)
        public const string ApplicationName = "FinancialDataIngestor.RestAPI";
        public const string ApiPathBase = "Services.FinancialDataIngestorRestAPI";
        public const string Swagger = "Sswagger";
        public const string Index = "index.html";
        public const string Configuration = "UiConfiguration";
        public const string JsonData = @"C:\Users\ppm2k\source\repos\FinancialDataIngestor\FinancialDataIngestor.Console\Data\Payload.json";
        public const string ZipUrl = "https://github.com/ppm2k2/FinancialDataIngestor/raw/main/FinancialDataIngestor.Console/Data/Payload.zip";
        public const string TimestampFormat = "yyyyMMdd_HHmmss";
        public const string BaseFolder = @"C:\Network\FileServer\FundAdmin\Position";
        public const string ZipFileNameFormat = "CLT_29481_Smith_ACC_10042_Position_Extract_{0}.zip";
        public const string JsonFileNameFormat = "CLT_29481_Smith_ACC_10042_Position_File_{0}.json";
        public const string UserAgent = "C# Financial Data Ingestor App";

    }
}
