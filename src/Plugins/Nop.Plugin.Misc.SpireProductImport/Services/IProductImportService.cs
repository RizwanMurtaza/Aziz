using Nop.Plugin.Misc.SpireProductImport.Models;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

/// <summary>
/// Represents a service for product import operations
/// </summary>
public interface IProductImportService
{
    /// <summary>
    /// Performs a full product import from the Spire FTP feed
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the import result</returns>
    Task<ImportResultModel> ImportProductsAsync();

    /// <summary>
    /// Downloads the latest CSV file from FTP
    /// </summary>
    /// <returns>A task that represents the asynchronous operation. The task result contains the local file path if successful, null otherwise</returns>
    Task<string> DownloadLatestCsvFileAsync();

    /// <summary>
    /// Processes a single product from the CSV data
    /// </summary>
    /// <param name="csvProduct">The CSV product data</param>
    /// <param name="settings">Import settings</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates if the processing was successful</returns>
    Task<bool> ProcessProductAsync(SpireProductCsvModel csvProduct, SpireProductImportSettings settings);

    /// <summary>
    /// Creates or updates categories from the CSV data
    /// </summary>
    /// <param name="categoryName">The category name</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the category ID</returns>
    Task<int> GetOrCreateCategoryAsync(string categoryName);

    /// <summary>
    /// Creates or updates manufacturers from the CSV data
    /// </summary>
    /// <param name="manufacturerName">The manufacturer name</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the manufacturer ID</returns>
    Task<int> GetOrCreateManufacturerAsync(string manufacturerName);

    /// <summary>
    /// Downloads and saves product images
    /// </summary>
    /// <param name="imageUrl">The image URL</param>
    /// <param name="productId">The product ID</param>
    /// <param name="isMainImage">Whether this is the main product image</param>
    /// <returns>A task that represents the asynchronous operation. The task result indicates if the download was successful</returns>
    Task<bool> DownloadProductImageAsync(string imageUrl, int productId, bool isMainImage = false);

    /// <summary>
    /// Marks products as discontinued if they're not in the current import
    /// </summary>
    /// <param name="activeProductNumbers">List of active product numbers from the current import</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the number of products marked as discontinued</returns>
    Task<int> MarkMissingProductsAsDiscontinuedAsync(List<string> activeProductNumbers);
}