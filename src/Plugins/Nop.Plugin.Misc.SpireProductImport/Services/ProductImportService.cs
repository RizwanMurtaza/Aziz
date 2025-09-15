using System.Text.RegularExpressions;
using Microsoft.Extensions.Logging;
using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.SpireProductImport.Models;
using Nop.Services.Catalog;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Services.Seo;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

/// <summary>
/// Represents product import service implementation
/// </summary>
public class ProductImportService : IProductImportService
{
    #region Fields

    private readonly ILogger<ProductImportService> _logger;
    private readonly ISettingService _settingService;
    private readonly IFtpService _ftpService;
    private readonly ICsvParsingService _csvParsingService;
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;
    private readonly IManufacturerService _manufacturerService;
    private readonly IPictureService _pictureService;
    private readonly IUrlRecordService _urlRecordService;
    private readonly INopFileProvider _fileProvider;
    private readonly IStoreContext _storeContext;

    #endregion

    #region Ctor

    public ProductImportService(
        ILogger<ProductImportService> logger,
        ISettingService settingService,
        IFtpService ftpService,
        ICsvParsingService csvParsingService,
        IProductService productService,
        ICategoryService categoryService,
        IManufacturerService manufacturerService,
        IPictureService pictureService,
        IUrlRecordService urlRecordService,
        INopFileProvider fileProvider,
        IStoreContext storeContext)
    {
        _logger = logger;
        _settingService = settingService;
        _ftpService = ftpService;
        _csvParsingService = csvParsingService;
        _productService = productService;
        _categoryService = categoryService;
        _manufacturerService = manufacturerService;
        _pictureService = pictureService;
        _urlRecordService = urlRecordService;
        _fileProvider = fileProvider;
        _storeContext = storeContext;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Performs a full product import from the Spire FTP feed
    /// </summary>
    public async Task<ImportResultModel> ImportProductsAsync()
    {
        var result = new ImportResultModel
        {
            ImportStartTime = DateTime.UtcNow
        };

        try
        {
            _logger.LogInformation("Starting Spire product import process");

            // Load settings
            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<SpireProductImportSettings>(store.Id);

            if (!settings.IsEnabled)
            {
                result.ErrorMessages.Add("Plugin is not enabled");
                result.IsSuccess = false;
                return result;
            }

            // Update last import time
            settings.LastImportDateTime = DateTime.UtcNow;
            await _settingService.SaveSettingAsync(settings);

            // Download CSV file
            var csvFilePath = await DownloadLatestCsvFileAsync();
            if (string.IsNullOrEmpty(csvFilePath))
            {
                result.ErrorMessages.Add("Failed to download CSV file from FTP");
                result.IsSuccess = false;
                return result;
            }

            // Validate CSV file
            var (isValid, validationErrors) = await _csvParsingService.ValidateCsvFileAsync(csvFilePath);
            if (!isValid)
            {
                result.ErrorMessages.AddRange(validationErrors);
                result.IsSuccess = false;
                return result;
            }

            // Parse CSV file
            var csvProducts = await _csvParsingService.ParseCsvFileAsync(csvFilePath);
            result.TotalProcessed = csvProducts.Count;

            _logger.LogInformation("Processing {Count} products from CSV file", csvProducts.Count);

            // Process each product
            var activeProductNumbers = new List<string>();

            foreach (var csvProduct in csvProducts)
            {
                try
                {
                    var success = await ProcessProductAsync(csvProduct, settings);
                    if (success)
                    {
                        result.SuccessfullyImported++;
                        activeProductNumbers.Add(csvProduct.ProductNumber);
                    }
                    else
                    {
                        result.ErrorCount++;
                    }
                }
                catch (Exception ex)
                {
                    result.ErrorCount++;
                    result.ErrorMessages.Add($"Error processing product {csvProduct.ProductNumber}: {ex.Message}");
                    _logger.LogError(ex, "Error processing product {ProductNumber}", csvProduct.ProductNumber);
                }
            }

            // Mark missing products as discontinued
            if (settings.MarkMissingProductsAsDiscontinued)
            {
                result.ProductsMarkedDiscontinued = await MarkMissingProductsAsDiscontinuedAsync(activeProductNumbers);
            }

            // Update successful import time
            settings.LastSuccessfulImportDateTime = DateTime.UtcNow;
            await _settingService.SaveSettingAsync(settings);

            result.IsSuccess = result.ErrorCount == 0 || (result.SuccessfullyImported > 0 && result.ErrorCount < result.TotalProcessed / 2);

            _logger.LogInformation("Spire product import completed. Success: {IsSuccess}, Processed: {TotalProcessed}, Successful: {SuccessfullyImported}, Errors: {ErrorCount}",
                result.IsSuccess, result.TotalProcessed, result.SuccessfullyImported, result.ErrorCount);
        }
        catch (Exception ex)
        {
            result.IsSuccess = false;
            result.ErrorMessages.Add($"Import failed with exception: {ex.Message}");
            _logger.LogError(ex, "Spire product import failed with exception");
        }
        finally
        {
            result.ImportEndTime = DateTime.UtcNow;
        }

        return result;
    }

    /// <summary>
    /// Downloads the latest CSV file from FTP
    /// </summary>
    public async Task<string> DownloadLatestCsvFileAsync()
    {
        try
        {
            var store = await _storeContext.GetCurrentStoreAsync();
            var settings = await _settingService.LoadSettingAsync<SpireProductImportSettings>(store.Id);

            // Use a cache directory in the application data folder
            var cacheDirectory = Path.Combine(Path.GetTempPath(), "SpireProductImport", "Cache");

            _logger.LogInformation("Downloading CSV file {CsvFileName} from {FtpServer} with 24-hour caching",
                settings.CsvFileName, settings.FtpServer);

            var cachedFilePath = await _ftpService.DownloadFileWithCachingAsync(
                settings.FtpServer,
                settings.FtpUsername,
                settings.FtpPassword,
                settings.CsvFileName,
                cacheDirectory);

            if (!string.IsNullOrEmpty(cachedFilePath) && File.Exists(cachedFilePath))
            {
                _logger.LogInformation("Successfully obtained CSV file: {FilePath}", cachedFilePath);
                return cachedFilePath;
            }
            else
            {
                _logger.LogError("Failed to download or cache CSV file {CsvFileName}", settings.CsvFileName);
                return null;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download CSV file from FTP with caching");
            return null;
        }
    }

    /// <summary>
    /// Processes a single product from the CSV data
    /// </summary>
    public async Task<bool> ProcessProductAsync(SpireProductCsvModel csvProduct, SpireProductImportSettings settings)
    {
        try
        {
            // Find existing product by Product Number (SKU)
            var existingProduct = await _productService.GetProductBySkuAsync(csvProduct.ProductNumber);

            if (existingProduct != null)
            {
                if (!settings.UpdateExistingProducts)
                {
                    _logger.LogDebug("Skipping existing product {ProductNumber} (update disabled)", csvProduct.ProductNumber);
                    return true;
                }

                // Update existing product
                await UpdateExistingProductAsync(existingProduct, csvProduct, settings);
                return true;
            }
            else
            {
                // Create new product
                await CreateNewProductAsync(csvProduct, settings);
                return true;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to process product {ProductNumber}", csvProduct.ProductNumber);
            return false;
        }
    }

    /// <summary>
    /// Creates a new product from CSV data
    /// </summary>
    private async Task CreateNewProductAsync(SpireProductCsvModel csvProduct, SpireProductImportSettings settings)
    {
        var product = new Product
        {
            ProductType = ProductType.SimpleProduct,
            VisibleIndividually = true,
            Name = csvProduct.Description,
            Sku = csvProduct.ProductNumber,
            ManufacturerPartNumber = csvProduct.ManufacturerNumber,
            ShortDescription = csvProduct.Description,
            FullDescription = csvProduct.ProductDetails,
            Price = CalculateSellingPrice(csvProduct.Cost, settings.MarkupPercentage),
            OldPrice = 0,
            ProductCost = csvProduct.Cost,
            CallForPrice = false,
            CustomerEntersPrice = false,
            Published = true,
            CreatedOnUtc = DateTime.UtcNow,
            UpdatedOnUtc = DateTime.UtcNow,
            Weight = csvProduct.Weight,
            StockQuantity = csvProduct.StockLevel,
            ManageInventoryMethod = ManageInventoryMethod.ManageStock,
            OrderMinimumQuantity = 1,
            OrderMaximumQuantity = 10000,
            AllowBackInStockSubscriptions = true,
            DisplayOrder = 0,
            SubjectToAcl = false,
            LimitedToStores = false,
            TaxCategoryId = 0 // You may want to set a specific tax category
        };

        // Set GTIN (EAN)
        if (!string.IsNullOrWhiteSpace(csvProduct.EanNumber))
        {
            product.Gtin = csvProduct.EanNumber;
        }

        // Save product
        await _productService.InsertProductAsync(product);

        // Create SEO-friendly URL
        var seName = await _urlRecordService.ValidateSeNameAsync(product, string.Empty, product.Name, true);
        await _urlRecordService.SaveSlugAsync(product, seName, 0);

        // Assign to category
        if (settings.AutoCreateCategories && !string.IsNullOrWhiteSpace(csvProduct.Category))
        {
            var categoryId = await GetOrCreateCategoryAsync(csvProduct.Category);
            if (categoryId > 0)
            {
                var productCategory = new ProductCategory
                {
                    ProductId = product.Id,
                    CategoryId = categoryId,
                    IsFeaturedProduct = false,
                    DisplayOrder = 0
                };
                await _categoryService.InsertProductCategoryAsync(productCategory);
            }
        }

        // Assign to manufacturer
        if (!string.IsNullOrWhiteSpace(csvProduct.Manufacturer))
        {
            var manufacturerId = await GetOrCreateManufacturerAsync(csvProduct.Manufacturer);
            if (manufacturerId > 0)
            {
                var productManufacturer = new ProductManufacturer
                {
                    ProductId = product.Id,
                    ManufacturerId = manufacturerId,
                    IsFeaturedProduct = false,
                    DisplayOrder = 0
                };
                await _manufacturerService.InsertProductManufacturerAsync(productManufacturer);
            }
        }

        // Handle images - either download or save URLs directly
        if (settings.DownloadProductImages)
        {
            // Download and save images locally
            if (!string.IsNullOrWhiteSpace(csvProduct.MainImage))
            {
                await DownloadProductImageAsync(csvProduct.MainImage, product.Id, true);
            }
            if (!string.IsNullOrWhiteSpace(csvProduct.Thumbnail) && csvProduct.Thumbnail != csvProduct.MainImage)
            {
                await DownloadProductImageAsync(csvProduct.Thumbnail, product.Id, false);
            }
        }
        else
        {
            // Save image URLs directly without downloading
            await SaveProductImageUrlsAsync(csvProduct.MainImage, csvProduct.Thumbnail, product.Id);
        }

        _logger.LogDebug("Created new product: {ProductNumber} - {Name}", product.Sku, product.Name);
    }

    /// <summary>
    /// Updates an existing product with CSV data
    /// </summary>
    private async Task UpdateExistingProductAsync(Product existingProduct, SpireProductCsvModel csvProduct, SpireProductImportSettings settings)
    {
        existingProduct.Name = csvProduct.Description;
        existingProduct.ManufacturerPartNumber = csvProduct.ManufacturerNumber;
        existingProduct.ShortDescription = csvProduct.Description;
        existingProduct.FullDescription = csvProduct.ProductDetails;
        existingProduct.Price = CalculateSellingPrice(csvProduct.Cost, settings.MarkupPercentage);
        existingProduct.ProductCost = csvProduct.Cost;
        existingProduct.UpdatedOnUtc = DateTime.UtcNow;
        existingProduct.Weight = csvProduct.Weight;
        existingProduct.StockQuantity = csvProduct.StockLevel;

        if (!string.IsNullOrWhiteSpace(csvProduct.EanNumber))
        {
            existingProduct.Gtin = csvProduct.EanNumber;
        }

        await _productService.UpdateProductAsync(existingProduct);

        // Handle images - either download or save URLs directly
        if (settings.DownloadProductImages)
        {
            // Download and save images locally
            if (!string.IsNullOrWhiteSpace(csvProduct.MainImage))
            {
                await DownloadProductImageAsync(csvProduct.MainImage, existingProduct.Id, true);
            }
            if (!string.IsNullOrWhiteSpace(csvProduct.Thumbnail) && csvProduct.Thumbnail != csvProduct.MainImage)
            {
                await DownloadProductImageAsync(csvProduct.Thumbnail, existingProduct.Id, false);
            }
        }
        else
        {
            // Save image URLs directly without downloading
            await SaveProductImageUrlsAsync(csvProduct.MainImage, csvProduct.Thumbnail, existingProduct.Id);
        }

        _logger.LogDebug("Updated existing product: {ProductNumber} - {Name}", existingProduct.Sku, existingProduct.Name);
    }

    /// <summary>
    /// Calculates selling price with markup
    /// </summary>
    private decimal CalculateSellingPrice(decimal cost, decimal markupPercentage)
    {
        return cost * (1 + markupPercentage / 100);
    }

    /// <summary>
    /// Creates or updates categories from the CSV data
    /// </summary>
    public async Task<int> GetOrCreateCategoryAsync(string categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return 0;

        try
        {
            // Check if category already exists
            var allCategories = await _categoryService.GetAllCategoriesAsync(showHidden: true);
            var existingCategory = allCategories.FirstOrDefault(c => c.Name.Equals(categoryName, StringComparison.OrdinalIgnoreCase));

            if (existingCategory != null)
                return existingCategory.Id;

            // Create new category
            var category = new Category
            {
                Name = categoryName,
                Description = $"Products in the {categoryName} category",
                Published = true,
                IncludeInTopMenu = true,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9, 18",
                PageSize = 6,
                PriceRangeFiltering = true,
                ManuallyPriceRange = false,
                PriceFrom = 0,
                PriceTo = 10000,
                ShowOnHomepage = false,
                DisplayOrder = 0,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                SubjectToAcl = false,
                LimitedToStores = false
            };

            await _categoryService.InsertCategoryAsync(category);

            // Create SEO-friendly URL
            var seName = await _urlRecordService.ValidateSeNameAsync(category, string.Empty, category.Name, true);
            await _urlRecordService.SaveSlugAsync(category, seName, 0);

            _logger.LogDebug("Created new category: {CategoryName}", categoryName);
            return category.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create category: {CategoryName}", categoryName);
            return 0;
        }
    }

    /// <summary>
    /// Creates or updates manufacturers from the CSV data
    /// </summary>
    public async Task<int> GetOrCreateManufacturerAsync(string manufacturerName)
    {
        if (string.IsNullOrWhiteSpace(manufacturerName))
            return 0;

        try
        {
            // Check if manufacturer already exists
            var allManufacturers = await _manufacturerService.GetAllManufacturersAsync(showHidden: true);
            var existingManufacturer = allManufacturers.FirstOrDefault(m => m.Name.Equals(manufacturerName, StringComparison.OrdinalIgnoreCase));

            if (existingManufacturer != null)
                return existingManufacturer.Id;

            // Create new manufacturer
            var manufacturer = new Manufacturer
            {
                Name = manufacturerName,
                Description = $"Products manufactured by {manufacturerName}",
                Published = true,
                AllowCustomersToSelectPageSize = true,
                PageSizeOptions = "6, 3, 9, 18",
                PageSize = 6,
                PriceRangeFiltering = true,
                ManuallyPriceRange = false,
                PriceFrom = 0,
                PriceTo = 10000,
                DisplayOrder = 0,
                CreatedOnUtc = DateTime.UtcNow,
                UpdatedOnUtc = DateTime.UtcNow,
                SubjectToAcl = false,
                LimitedToStores = false
            };

            await _manufacturerService.InsertManufacturerAsync(manufacturer);

            // Create SEO-friendly URL
            var seName = await _urlRecordService.ValidateSeNameAsync(manufacturer, string.Empty, manufacturer.Name, true);
            await _urlRecordService.SaveSlugAsync(manufacturer, seName, 0);

            _logger.LogDebug("Created new manufacturer: {ManufacturerName}", manufacturerName);
            return manufacturer.Id;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create manufacturer: {ManufacturerName}", manufacturerName);
            return 0;
        }
    }

    /// <summary>
    /// Downloads and saves product images
    /// </summary>
    public async Task<bool> DownloadProductImageAsync(string imageUrl, int productId, bool isMainImage = false)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(imageUrl))
                return false;

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromMinutes(2);

            var imageBytes = await httpClient.GetByteArrayAsync(imageUrl);

            if (imageBytes?.Length > 0)
            {
                // Extract file extension from URL
                var fileName = Path.GetFileName(new Uri(imageUrl).LocalPath);
                var extension = Path.GetExtension(fileName);

                if (string.IsNullOrWhiteSpace(extension))
                    extension = ".jpg"; // Default extension

                var mimeType = GetMimeType(extension);
                var displayOrder = isMainImage ? 0 : 1;

                var picture = await _pictureService.InsertPictureAsync(imageBytes, mimeType, fileName);

                if (picture != null)
                {
                    var productPicture = new ProductPicture
                    {
                        PictureId = picture.Id,
                        ProductId = productId,
                        DisplayOrder = displayOrder
                    };

                    await _productService.InsertProductPictureAsync(productPicture);

                    _logger.LogDebug("Downloaded and saved product image for product {ProductId}: {ImageUrl}", productId, imageUrl);
                    return true;
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to download product image for product {ProductId}: {ImageUrl}", productId, imageUrl);
        }

        return false;
    }

    /// <summary>
    /// Saves product image URLs directly without downloading
    /// </summary>
    private async Task<bool> SaveProductImageUrlsAsync(string mainImageUrl, string thumbnailUrl, int productId)
    {
        try
        {
            // Remove existing product pictures first to avoid duplicates
            var existingPictures = await _productService.GetProductPicturesByProductIdAsync(productId);
            foreach (var existingPicture in existingPictures)
            {
                await _productService.DeleteProductPictureAsync(existingPicture);
            }

            var picturesCreated = 0;

            // Create picture record for main image URL
            if (!string.IsNullOrWhiteSpace(mainImageUrl))
            {
                // Create a "virtual" picture entry with the external URL stored in VirtualPath
                var mainPicture = await _pictureService.InsertPictureAsync(
                    pictureBinary: new byte[0], // Empty byte array for external URLs
                    mimeType: GetMimeTypeFromUrl(mainImageUrl),
                    seoFilename: GetSeoFriendlyFileName(mainImageUrl),
                    altAttribute: "Product Image",
                    titleAttribute: "Product Image",
                    isNew: true,
                    validateBinary: false
                );

                if (mainPicture != null)
                {
                    // Update the picture to store the external URL in VirtualPath
                    mainPicture.VirtualPath = mainImageUrl;
                    await _pictureService.UpdatePictureAsync(mainPicture);

                    var productPicture = new ProductPicture
                    {
                        PictureId = mainPicture.Id,
                        ProductId = productId,
                        DisplayOrder = 0
                    };

                    await _productService.InsertProductPictureAsync(productPicture);
                    picturesCreated++;

                    _logger.LogDebug("Saved main image URL for product {ProductId}: {ImageUrl}", productId, mainImageUrl);
                }
            }

            // Create picture record for thumbnail URL (if different from main)
            if (!string.IsNullOrWhiteSpace(thumbnailUrl) && thumbnailUrl != mainImageUrl)
            {
                var thumbnailPicture = await _pictureService.InsertPictureAsync(
                    pictureBinary: new byte[0], // Empty byte array for external URLs
                    mimeType: GetMimeTypeFromUrl(thumbnailUrl),
                    seoFilename: GetSeoFriendlyFileName(thumbnailUrl),
                    altAttribute: "Product Thumbnail",
                    titleAttribute: "Product Thumbnail",
                    isNew: true,
                    validateBinary: false
                );

                if (thumbnailPicture != null)
                {
                    // Update the picture to store the external URL in VirtualPath
                    thumbnailPicture.VirtualPath = thumbnailUrl;
                    await _pictureService.UpdatePictureAsync(thumbnailPicture);

                    var productPicture = new ProductPicture
                    {
                        PictureId = thumbnailPicture.Id,
                        ProductId = productId,
                        DisplayOrder = 1
                    };

                    await _productService.InsertProductPictureAsync(productPicture);
                    picturesCreated++;

                    _logger.LogDebug("Saved thumbnail URL for product {ProductId}: {ImageUrl}", productId, thumbnailUrl);
                }
            }

            return picturesCreated > 0;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to save image URLs for product {ProductId}. Main: {MainUrl}, Thumbnail: {ThumbnailUrl}",
                productId, mainImageUrl, thumbnailUrl);
            return false;
        }
    }

    /// <summary>
    /// Gets SEO-friendly filename from URL
    /// </summary>
    private string GetSeoFriendlyFileName(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return "image";

        try
        {
            var uri = new Uri(imageUrl);
            var fileName = Path.GetFileNameWithoutExtension(uri.LocalPath);
            if (string.IsNullOrWhiteSpace(fileName))
                return "image";

            // Make it SEO-friendly
            return Regex.Replace(fileName.ToLowerInvariant(), @"[^a-z0-9\-]", "-");
        }
        catch
        {
            return "image";
        }
    }

    /// <summary>
    /// Gets MIME type from URL
    /// </summary>
    private string GetMimeTypeFromUrl(string imageUrl)
    {
        if (string.IsNullOrWhiteSpace(imageUrl))
            return MimeTypes.ImageJpeg;

        try
        {
            var uri = new Uri(imageUrl);
            var extension = Path.GetExtension(uri.LocalPath);
            return GetMimeType(extension);
        }
        catch
        {
            return MimeTypes.ImageJpeg;
        }
    }

    /// <summary>
    /// Gets MIME type from file extension
    /// </summary>
    private string GetMimeType(string extension)
    {
        return extension.ToLowerInvariant() switch
        {
            ".jpg" or ".jpeg" => MimeTypes.ImageJpeg,
            ".png" => MimeTypes.ImagePng,
            ".gif" => MimeTypes.ImageGif,
            ".webp" => "image/webp",
            _ => MimeTypes.ImageJpeg
        };
    }

    /// <summary>
    /// Marks products as discontinued if they're not in the current import
    /// </summary>
    public async Task<int> MarkMissingProductsAsDiscontinuedAsync(List<string> activeProductNumbers)
    {
        try
        {
            _logger.LogInformation("Marking products as discontinued that are not in the current import");

            var discontinuedCount = 0;

            // Get all products that were previously imported (have SKUs starting with our pattern or specific to Spire)
            var allProducts = await _productService.SearchProductsAsync(
                showHidden: true,
                pageSize: int.MaxValue
            );

            foreach (var product in allProducts.Where(p => !string.IsNullOrWhiteSpace(p.Sku)))
            {
                if (!activeProductNumbers.Contains(product.Sku))
                {
                    // Mark as discontinued (unpublished)
                    product.Published = false;
                    product.UpdatedOnUtc = DateTime.UtcNow;
                    await _productService.UpdateProductAsync(product);
                    discontinuedCount++;
                }
            }

            _logger.LogInformation("Marked {Count} products as discontinued", discontinuedCount);
            return discontinuedCount;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to mark missing products as discontinued");
            return 0;
        }
    }

    #endregion
}