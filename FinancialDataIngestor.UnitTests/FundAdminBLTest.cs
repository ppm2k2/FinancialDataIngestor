using FinancialDataIngestor.Models.Type;
using FundAdminRestAPI.Interfaces.DataAccess;
using FundAdminRestAPI.Models;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Reflection;
using System.Text.Json;
using Xunit;

namespace FinancialDataIngestor.BusinessLogic.Tests
{
    public class FundAdminBLTests
    {
        private static Type? GetConstantsType()
        {
            // Attempt to find a type named "Constants" in the same assembly as FundAdminBL
            var assembly = typeof(FundAdminBL).Assembly;
            return assembly.GetTypes().FirstOrDefault(t => t.Name == "Constants");
        }

        private static (MemberInfo? member, bool isLiteral, string? currentValue) GetBaseFolderMember()
        {
            var constantsType = GetConstantsType();
            if (constantsType == null) return (null, false, null);

            var field = constantsType.GetField("BaseFolder", BindingFlags.Public | BindingFlags.Static);
            if (field != null)
            {
                bool isLiteral = field.IsLiteral;
                // Use 'as string' to avoid converting a possible null to non-nullable
                string? current = isLiteral ? field.GetRawConstantValue() as string : field.GetValue(null) as string;
                return (field, isLiteral, current);
            }

            var prop = constantsType.GetProperty("BaseFolder", BindingFlags.Public | BindingFlags.Static);
            if (prop != null)
            {
                // properties cannot be literal consts
                string? current = prop.GetValue(null) as string;
                return (prop, false, current);
            }

            return (null, false, null);
        }

        private static void SetBaseFolder(MemberInfo? member, string? value)
        {
            if (member == null) return;
            if (member is FieldInfo f && !f.IsLiteral)
                f.SetValue(null, value);
            else if (member is PropertyInfo p && p.CanWrite)
                p.SetValue(null, value);
        }

        private static void RestoreBaseFolder(MemberInfo? member, string? original)
        {
            SetBaseFolder(member, original);
        }

        [Fact]
        public void ReadFundData_Returns_FirstClient_WhenZipContainsJson()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "FundAdminBLTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var (member, isLiteral, original) = GetBaseFolderMember();

            // Determine folder to use (if BaseFolder is const literal, we must use original folder)
            string folderToUse = isLiteral && original != null ? original : tempDir;
            bool restored = false;

            try
            {
                if (!isLiteral && member != null)
                {
                    // set BaseFolder to tempDir
                    SetBaseFolder(member, tempDir);
                    restored = true;
                }

                // Prepare JSON with a single ClientAccountDTO
                var sample = new[] { new ClientAccountDTO { ClientId = "CLT_123" } };
                string json = JsonSerializer.Serialize(sample);

                // Create zip in folderToUse containing data.json
                string zipPath = Path.Combine(folderToUse, $"test_{DateTime.UtcNow:yyyyMMddHHmmssfff}.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    var entry = zip.CreateEntry("data.json");
                    using var s = new StreamWriter(entry.Open());
                    s.Write(json);
                }

                // Use null-forgiving operator to suppress nullable-conversion warning for test construction
                var bl = new FundAdminBL(null!);
                var result = bl.ReadFundData();

                Xunit.Assert.NotNull(result);
                Xunit.Assert.Equal("CLT_123", result.ClientId);
            }
            finally
            {
                if (restored)
                    RestoreBaseFolder(member, original);

                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        [Fact]
        public void ReadFundData_Throws_WhenNoZipPresent()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "FundAdminBLTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var (member, isLiteral, original) = GetBaseFolderMember();
            string folderToUse = isLiteral && original != null ? original : tempDir;
            bool restored = false;

            try
            {
                if (!isLiteral && member != null)
                {
                    SetBaseFolder(member, tempDir);
                    restored = true;
                }

                // Ensure folderToUse has no zip files
                foreach (var f in Directory.GetFiles(folderToUse, "*.zip")) File.Delete(f);

                var bl = new FundAdminBL(null!);

                Xunit.Assert.Throws<FileNotFoundException>(() => bl.ReadFundData());
            }
            finally
            {
                if (restored)
                    RestoreBaseFolder(member, original);

                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }

        [Fact]
        public void ReadFundData_Throws_WhenZipHasNoJson()
        {
            string tempDir = Path.Combine(Path.GetTempPath(), "FundAdminBLTests", Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(tempDir);

            var (member, isLiteral, original) = GetBaseFolderMember();
            string folderToUse = isLiteral && original != null ? original : tempDir;
            bool restored = false;

            try
            {
                if (!isLiteral && member != null)
                {
                    SetBaseFolder(member, tempDir);
                    restored = true;
                }

                // Create a zip with a non-json file
                string zipPath = Path.Combine(folderToUse, $"nojson_{DateTime.UtcNow:yyyyMMddHHmmssfff}.zip");
                using (var zip = ZipFile.Open(zipPath, ZipArchiveMode.Create))
                {
                    var entry = zip.CreateEntry("readme.txt");
                    using var s = new StreamWriter(entry.Open());
                    s.Write("no json here");
                }

                var bl = new FundAdminBL(null!);

                Xunit.Assert.Throws<FileNotFoundException>(() => bl.ReadFundData());
            }
            finally
            {
                if (restored)
                    RestoreBaseFolder(member, original);

                try { Directory.Delete(tempDir, recursive: true); } catch { }
            }
        }
    }
}