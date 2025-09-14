using System.Globalization;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Logging;
using Nop.Plugin.Misc.SpireProductImport.Models;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

/// <summary>
/// Represents CSV parsing service implementation
/// </summary>
public class CsvParsingService : ICsvParsingService
{
    #region Fields

    private readonly ILogger<CsvParsingService> _logger;

    #endregion

    #region Ctor

    public CsvParsingService(ILogger<CsvParsingService> logger)
    {
        _logger = logger;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Parses CSV file and returns list of Spire products
    /// </summary>
    public async Task<List<SpireProductCsvModel>> ParseCsvFileAsync(string csvFilePath)
    {
        var products = new List<SpireProductCsvModel>();

        try
        {
            _logger.LogInformation("Starting CSV parsing for file: {CsvFile}", csvFilePath);

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim,
                BadDataFound = null
            };

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, config);

            // Register custom converter for decimal fields that might have currency symbols or commas
            csv.Context.TypeConverterOptionsCache.GetOptions<decimal>().NumberStyles = NumberStyles.Any;

            var rowNumber = 1; // Start at 1 since header is row 0

            while (await csv.ReadAsync())
            {
                rowNumber++;
                try
                {
                    var product = csv.GetRecord<SpireProductCsvModel>();

                    // Basic validation
                    if (!string.IsNullOrWhiteSpace(product?.ProductNumber))
                    {
                        products.Add(product);
                    }
                    else
                    {
                        _logger.LogWarning("Skipping product with empty ProductNumber at row {Row}", rowNumber);
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogWarning("Failed to parse product at row {Row}: {Error}. Skipping this product.", rowNumber, ex.Message);
                    // Continue processing the next record instead of failing completely
                    continue;
                }
            }

            _logger.LogInformation("Successfully parsed {Count} products from CSV file", products.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to parse CSV file {CsvFile}: {Error}", csvFilePath, ex.Message);
            throw;
        }

        return products;
    }

    /// <summary>
    /// Validates the CSV file format
    /// </summary>
    public async Task<(bool isValid, List<string> errors)> ValidateCsvFileAsync(string csvFilePath)
    {
        var errors = new List<string>();
        var isValid = true;

        try
        {
            _logger.LogInformation("Validating CSV file format: {CsvFile}", csvFilePath);

            if (!File.Exists(csvFilePath))
            {
                errors.Add($"CSV file not found: {csvFilePath}");
                return (false, errors);
            }

            var config = new CsvConfiguration(CultureInfo.InvariantCulture)
            {
                HasHeaderRecord = true,
                MissingFieldFound = null,
                HeaderValidated = null,
                TrimOptions = TrimOptions.Trim
            };

            using var reader = new StreamReader(csvFilePath);
            using var csv = new CsvReader(reader, config);

            // Read header
            await csv.ReadAsync();
            csv.ReadHeader();

            // Check required columns
            var requiredColumns = new[]
            {
                "Product Number", "Manufacturer Number", "Category", "Manufacturer",
                "Description", "Cost", "Stock Level", "Product Details", "Specification",
                "EAN Number", "Thumbnail", "Main Image", "Weight"
            };

            foreach (var column in requiredColumns)
            {
                if (!csv.HeaderRecord.Contains(column))
                {
                    errors.Add($"Missing required column: {column}");
                    isValid = false;
                }
            }

            // Validate first few records
            var recordCount = 0;
            var maxValidationRecords = 10;

            while (await csv.ReadAsync() && recordCount < maxValidationRecords)
            {
                recordCount++;
                try
                {
                    var record = csv.GetRecord<SpireProductCsvModel>();

                    if (string.IsNullOrWhiteSpace(record.ProductNumber))
                        errors.Add($"Empty Product Number at row {recordCount + 1}");

                    if (string.IsNullOrWhiteSpace(record.Description))
                        errors.Add($"Empty Description at row {recordCount + 1}");

                    if (record.Cost <= 0)
                        errors.Add($"Invalid Cost value at row {recordCount + 1}");
                }
                catch (Exception ex)
                {
                    errors.Add($"Error parsing row {recordCount + 1}: {ex.Message}");
                    isValid = false;
                }
            }

            _logger.LogInformation("CSV validation completed. IsValid: {IsValid}, Errors: {ErrorCount}", isValid, errors.Count);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to validate CSV file {CsvFile}: {Error}", csvFilePath, ex.Message);
            errors.Add($"Validation error: {ex.Message}");
            isValid = false;
        }

        return (isValid, errors);
    }

    /// <summary>
    /// Gets the total record count in the CSV file without loading all data
    /// </summary>
    public async Task<int> GetRecordCountAsync(string csvFilePath)
    {
        try
        {
            _logger.LogDebug("Counting records in CSV file: {CsvFile}", csvFilePath);

            if (!File.Exists(csvFilePath))
                return 0;

            using var reader = new StreamReader(csvFilePath);

            var lineCount = 0;
            while (!reader.EndOfStream)
            {
                await reader.ReadLineAsync();
                lineCount++;
            }

            // Subtract 1 for header row
            var recordCount = Math.Max(0, lineCount - 1);

            _logger.LogDebug("CSV file contains {RecordCount} data records", recordCount);
            return recordCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to count records in CSV file {CsvFile}: {Error}", csvFilePath, ex.Message);
            return 0;
        }
    }

    #endregion
}