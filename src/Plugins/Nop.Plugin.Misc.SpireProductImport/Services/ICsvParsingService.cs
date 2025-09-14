using Nop.Plugin.Misc.SpireProductImport.Models;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

/// <summary>
/// Represents a service for CSV parsing operations
/// </summary>
public interface ICsvParsingService
{
    /// <summary>
    /// Parses CSV file and returns list of Spire products
    /// </summary>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains list of parsed products</returns>
    Task<List<SpireProductCsvModel>> ParseCsvFileAsync(string csvFilePath);

    /// <summary>
    /// Validates the CSV file format
    /// </summary>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains validation result</returns>
    Task<(bool isValid, List<string> errors)> ValidateCsvFileAsync(string csvFilePath);

    /// <summary>
    /// Gets the total record count in the CSV file without loading all data
    /// </summary>
    /// <param name="csvFilePath">Path to the CSV file</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the record count</returns>
    Task<int> GetRecordCountAsync(string csvFilePath);
}