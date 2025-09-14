using Microsoft.Extensions.Logging;
using Nop.Plugin.Misc.SpireProductImport.Services;
using Nop.Services.Configuration;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

/// <summary>
/// Represents the scheduled task for automatic product import
/// </summary>
public class SpireProductImportTask : IScheduleTask
{
    #region Fields

    private readonly ILogger<SpireProductImportTask> _logger;
    private readonly IProductImportService _productImportService;
    private readonly ISettingService _settingService;

    #endregion

    #region Ctor

    public SpireProductImportTask(
        ILogger<SpireProductImportTask> logger,
        IProductImportService productImportService,
        ISettingService settingService)
    {
        _logger = logger;
        _productImportService = productImportService;
        _settingService = settingService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Executes the scheduled task
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task ExecuteAsync()
    {
        try
        {
            _logger.LogInformation("Starting automatic Spire product import task");

            // Get plugin settings
            var settings = await _settingService.LoadSettingAsync<SpireProductImportSettings>();

            // Check if plugin and automatic import are enabled
            if (!settings.IsEnabled)
            {
                _logger.LogInformation("Spire product import plugin is disabled. Skipping task execution.");
                return;
            }

            if (!settings.EnableAutomaticImport)
            {
                _logger.LogInformation("Automatic import is disabled. Skipping task execution.");
                return;
            }

            // Check if enough time has passed since last import (based on interval setting)
            var lastImport = settings.LastImportDateTime ?? DateTime.MinValue;
            var intervalHours = Math.Max(1, Math.Min(24, settings.ImportIntervalHours)); // Ensure 1-24 hours range
            var nextImportTime = lastImport.AddHours(intervalHours);

            if (DateTime.UtcNow < nextImportTime)
            {
                _logger.LogInformation("Import interval not reached. Next import scheduled for: {NextImportTime}",
                    nextImportTime.ToString("yyyy-MM-dd HH:mm:ss UTC"));
                return;
            }

            // Perform the import
            var importResult = await _productImportService.ImportProductsAsync();

            if (importResult.IsSuccess)
            {
                _logger.LogInformation("Automatic import completed successfully. Products processed: {ProductsProcessed}, " +
                    "New products: {NewProducts}, Updated products: {UpdatedProducts}, Errors: {ErrorCount}",
                    importResult.TotalProcessed, importResult.NewProductsCreated,
                    importResult.ExistingProductsUpdated, importResult.ErrorCount);

                // Update last successful import time
                settings.LastSuccessfulImportDateTime = DateTime.UtcNow;
                await _settingService.SaveSettingAsync(settings);
            }
            else
            {
                _logger.LogError("Automatic import failed. Errors: {ErrorCount}. Messages: {ErrorMessages}",
                    importResult.ErrorCount, string.Join(", ", importResult.ErrorMessages));
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during automatic Spire product import task");
        }
    }

    #endregion
}