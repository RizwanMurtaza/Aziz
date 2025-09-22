using Microsoft.AspNetCore.Mvc;
using Nop.Core;
using Nop.Plugin.Misc.SpireProductImport.Models;
using Nop.Plugin.Misc.SpireProductImport.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.SpireProductImport.Controllers;

[AuthorizeAdmin]
[Area(AreaNames.ADMIN)]
[AutoValidateAntiforgeryToken]
public class SpireProductImportController : BasePluginController
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly INotificationService _notificationService;
    private readonly ISettingService _settingService;
    private readonly IStoreContext _storeContext;
    private readonly IFtpService _ftpService;
    private readonly IProductImportService _productImportService;
    private readonly IProductExternalImageService _productExternalImageService;

    #endregion

    #region Ctor

    public SpireProductImportController(
        ILocalizationService localizationService,
        INotificationService notificationService,
        ISettingService settingService,
        IStoreContext storeContext,
        IFtpService ftpService,
        IProductImportService productImportService,
        IProductExternalImageService productExternalImageService)
    {
        _localizationService = localizationService;
        _notificationService = notificationService;
        _settingService = settingService;
        _storeContext = storeContext;
        _ftpService = ftpService;
        _productImportService = productImportService;
        _productExternalImageService = productExternalImageService;
    }

    #endregion

    #region Methods

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure()
    {
        // Load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<SpireProductImportSettings>(storeScope);

        var model = new ConfigurationModel
        {
            FtpServer = settings.FtpServer,
            FtpUsername = settings.FtpUsername,
            FtpPassword = settings.FtpPassword,
            CsvFileName = settings.CsvFileName,
            XmlFileName = settings.XmlFileName,
            IsEnabled = settings.IsEnabled,
            EnableAutomaticImport = settings.EnableAutomaticImport,
            ImportIntervalHours = settings.ImportIntervalHours,
            MarkupPercentage = settings.MarkupPercentage,
            AutoCreateCategories = settings.AutoCreateCategories,
            UpdateExistingProducts = settings.UpdateExistingProducts,
            MarkMissingProductsAsDiscontinued = settings.MarkMissingProductsAsDiscontinued,
            EnableDetailedLogging = settings.EnableDetailedLogging,
            DownloadProductImages = settings.DownloadProductImages,
            LastImportDateTime = settings.LastImportDateTime,
            LastSuccessfulImportDateTime = settings.LastSuccessfulImportDateTime,
            ActiveStoreScopeConfiguration = storeScope
        };

        // Set import status
        if (settings.LastImportDateTime.HasValue)
        {
            var timeSinceLastImport = DateTime.UtcNow - settings.LastImportDateTime.Value;
            if (settings.LastSuccessfulImportDateTime == settings.LastImportDateTime)
                model.ImportStatus = $"Last successful import: {timeSinceLastImport.TotalHours:F1} hours ago";
            else
                model.ImportStatus = $"Last import failed: {timeSinceLastImport.TotalHours:F1} hours ago";
        }
        else
        {
            model.ImportStatus = "Never imported";
        }

        if (storeScope > 0)
        {
            model.FtpServer_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpServer, storeScope);
            model.FtpUsername_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpUsername, storeScope);
            model.FtpPassword_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpPassword, storeScope);
            model.CsvFileName_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CsvFileName, storeScope);
            model.XmlFileName_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.XmlFileName, storeScope);
            model.IsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsEnabled, storeScope);
            model.EnableAutomaticImport_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableAutomaticImport, storeScope);
            model.ImportIntervalHours_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ImportIntervalHours, storeScope);
            model.MarkupPercentage_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MarkupPercentage, storeScope);
            model.AutoCreateCategories_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AutoCreateCategories, storeScope);
            model.UpdateExistingProducts_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UpdateExistingProducts, storeScope);
            model.MarkMissingProductsAsDiscontinued_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MarkMissingProductsAsDiscontinued, storeScope);
            model.EnableDetailedLogging_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableDetailedLogging, storeScope);
            model.DownloadProductImages_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DownloadProductImages, storeScope);
        }

        return View("~/Plugins/Misc.SpireProductImport/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> Configure(ConfigurationModel model, string command = "")
    {
        // Check if test connection button was clicked
        if (command == "test-connection" || Request.Form.ContainsKey("test-connection"))
        {
            return await TestConnection(model);
        }

        // Check if run import button was clicked
        if (command == "run-import" || Request.Form.ContainsKey("run-import"))
        {
            // First save the current settings, then run import
            await SaveSettings(model);
            return await RunImportNow();
        }

        // Check if save button was clicked
        if (command == "save" || Request.Form.ContainsKey("save") || string.IsNullOrEmpty(command))
        {
            return await SaveSettingsAndRedirect(model);
        }

        // This should not be reached due to the checks above
        return await Configure();
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> TestConnection(ConfigurationModel model)
    {
        try
        {
            // Ensure password is not empty - if empty, load from settings
            if (string.IsNullOrEmpty(model.FtpPassword))
            {
                var currentStoreScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
                var currentSettings = await _settingService.LoadSettingAsync<SpireProductImportSettings>(currentStoreScope);
                model.FtpPassword = currentSettings.FtpPassword;
            }

            var connectionTest = await _ftpService.TestConnectionAsync(model.FtpServer, model.FtpUsername, model.FtpPassword);

            if (connectionTest)
            {
                // Get file list to verify access
                var fileList = await _ftpService.GetFileListAsync(model.FtpServer, model.FtpUsername, model.FtpPassword);
                var hasRequiredFiles = fileList.Any(f => f.Equals(model.CsvFileName, StringComparison.OrdinalIgnoreCase));

                if (hasRequiredFiles)
                    _notificationService.SuccessNotification("FTP connection successful! Required files are accessible.");
                else
                    _notificationService.WarningNotification($"FTP connection successful, but {model.CsvFileName} was not found in the file list.");
            }
            else
            {
                _notificationService.ErrorNotification("FTP connection failed. Please check your credentials and server settings.");
            }
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"FTP connection test failed: {ex.Message}");
        }

        // PRESERVE ALL MODEL VALUES - don't reload from settings
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<SpireProductImportSettings>(storeScope);

        // Only update status-related fields, preserve form input values
        model.ActiveStoreScopeConfiguration = storeScope;
        model.LastImportDateTime = settings.LastImportDateTime;
        model.LastSuccessfulImportDateTime = settings.LastSuccessfulImportDateTime;

        // Ensure password is always available in the model for display
        if (string.IsNullOrEmpty(model.FtpPassword))
        {
            model.FtpPassword = settings.FtpPassword;
        }

        // Set import status
        if (settings.LastImportDateTime.HasValue)
        {
            var timeSinceLastImport = DateTime.UtcNow - settings.LastImportDateTime.Value;
            if (settings.LastSuccessfulImportDateTime == settings.LastImportDateTime)
                model.ImportStatus = $"Last successful import: {timeSinceLastImport.TotalHours:F1} hours ago";
            else
                model.ImportStatus = $"Last import failed: {timeSinceLastImport.TotalHours:F1} hours ago";
        }
        else
        {
            model.ImportStatus = "Never imported";
        }

        // Only set override flags if store scope > 0
        if (storeScope > 0)
        {
            model.FtpServer_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpServer, storeScope);
            model.FtpUsername_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpUsername, storeScope);
            model.FtpPassword_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.FtpPassword, storeScope);
            model.CsvFileName_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.CsvFileName, storeScope);
            model.XmlFileName_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.XmlFileName, storeScope);
            model.IsEnabled_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.IsEnabled, storeScope);
            model.EnableAutomaticImport_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableAutomaticImport, storeScope);
            model.ImportIntervalHours_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.ImportIntervalHours, storeScope);
            model.MarkupPercentage_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MarkupPercentage, storeScope);
            model.AutoCreateCategories_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.AutoCreateCategories, storeScope);
            model.UpdateExistingProducts_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.UpdateExistingProducts, storeScope);
            model.MarkMissingProductsAsDiscontinued_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.MarkMissingProductsAsDiscontinued, storeScope);
            model.EnableDetailedLogging_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.EnableDetailedLogging, storeScope);
            model.DownloadProductImages_OverrideForStore = await _settingService.SettingExistsAsync(settings, x => x.DownloadProductImages, storeScope);
        }

        return View("~/Plugins/Misc.SpireProductImport/Views/Configure.cshtml", model);
    }

    [HttpPost]
    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> RunImportNow()
    {
        try
        {
            _notificationService.SuccessNotification("Import process started. This may take several minutes to complete...");

            // Run the import process
            var importResult = await _productImportService.ImportProductsAsync();

            if (importResult.IsSuccess)
            {
                _notificationService.SuccessNotification($"Import completed successfully! {importResult.SummaryMessage}");
            }
            else
            {
                _notificationService.ErrorNotification($"Import failed with {importResult.ErrorCount} errors. Check logs for details.");
            }

            // Use explicit redirect to avoid routing issues
            return RedirectToAction("Configure");
        }
        catch (Exception ex)
        {
            _notificationService.ErrorNotification($"Failed to start import: {ex.Message}");
            return RedirectToAction("Configure");
        }
    }

    #endregion

    #region Helper Methods

    private async Task SaveSettings(ConfigurationModel model)
    {
        if (!ModelState.IsValid)
            return;

        // Load settings for a chosen store scope
        var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
        var settings = await _settingService.LoadSettingAsync<SpireProductImportSettings>(storeScope);

        settings.FtpServer = model.FtpServer;
        settings.FtpUsername = model.FtpUsername;
        settings.FtpPassword = model.FtpPassword;
        settings.CsvFileName = model.CsvFileName;
        settings.XmlFileName = model.XmlFileName;
        settings.IsEnabled = model.IsEnabled;
        settings.EnableAutomaticImport = model.EnableAutomaticImport;
        settings.ImportIntervalHours = model.ImportIntervalHours;
        settings.MarkupPercentage = model.MarkupPercentage;
        settings.AutoCreateCategories = model.AutoCreateCategories;
        settings.UpdateExistingProducts = model.UpdateExistingProducts;
        settings.MarkMissingProductsAsDiscontinued = model.MarkMissingProductsAsDiscontinued;
        settings.EnableDetailedLogging = model.EnableDetailedLogging;
        settings.DownloadProductImages = model.DownloadProductImages;

        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.FtpServer, model.FtpServer_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.FtpUsername, model.FtpUsername_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.FtpPassword, model.FtpPassword_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.CsvFileName, model.CsvFileName_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.XmlFileName, model.XmlFileName_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.IsEnabled, model.IsEnabled_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableAutomaticImport, model.EnableAutomaticImport_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.ImportIntervalHours, model.ImportIntervalHours_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MarkupPercentage, model.MarkupPercentage_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.AutoCreateCategories, model.AutoCreateCategories_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.UpdateExistingProducts, model.UpdateExistingProducts_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.MarkMissingProductsAsDiscontinued, model.MarkMissingProductsAsDiscontinued_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.EnableDetailedLogging, model.EnableDetailedLogging_OverrideForStore, storeScope, false);
        await _settingService.SaveSettingOverridablePerStoreAsync(settings, x => x.DownloadProductImages, model.DownloadProductImages_OverrideForStore, storeScope, false);

        // Clear settings cache
        await _settingService.ClearCacheAsync();
    }

    private async Task<IActionResult> SaveSettingsAndRedirect(ConfigurationModel model)
    {
        await SaveSettings(model);
        _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));
        return RedirectToAction("Configure");
    }

    [CheckPermission(StandardPermission.Configuration.MANAGE_PLUGINS)]
    public async Task<IActionResult> TestExternalImages()
    {
        var message = "External Image Service Test:\n\n";

        try
        {
            // Test service availability
            if (_productExternalImageService == null)
            {
                message += "❌ External Image Service is NOT available\n";
            }
            else
            {
                message += "✅ External Image Service is available\n\n";

                // Test with a few product IDs
                for (int productId = 1; productId <= 10; productId++)
                {
                    var externalImages = await _productExternalImageService.GetProductExternalImagesAsync(productId);
                    if (externalImages != null && (!string.IsNullOrEmpty(externalImages.MainImageUrl) || !string.IsNullOrEmpty(externalImages.ThumbnailUrl)))
                    {
                        message += $"Product {productId}:\n";
                        if (!string.IsNullOrEmpty(externalImages.MainImageUrl))
                            message += $"  Main Image: {externalImages.MainImageUrl}\n";
                        if (!string.IsNullOrEmpty(externalImages.ThumbnailUrl))
                            message += $"  Thumbnail: {externalImages.ThumbnailUrl}\n";
                        message += "\n";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            message += $"❌ Error: {ex.Message}\n";
        }

        return Content(message, "text/plain");
    }

    #endregion
}