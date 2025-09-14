using Nop.Core;
using Nop.Core.Domain.ScheduleTasks;
using Nop.Plugin.Misc.SpireProductImport.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.SpireProductImport;

/// <summary>
/// Represents Spire Product Import plugin
/// </summary>
public class SpireProductImportPlugin : BasePlugin
{
    #region Fields

    private readonly ILocalizationService _localizationService;
    private readonly ISettingService _settingService;
    private readonly IWebHelper _webHelper;
    private readonly IScheduleTaskService _scheduleTaskService;

    #endregion

    #region Ctor

    public SpireProductImportPlugin(
        ILocalizationService localizationService,
        ISettingService settingService,
        IWebHelper webHelper,
        IScheduleTaskService scheduleTaskService)
    {
        _localizationService = localizationService;
        _settingService = settingService;
        _webHelper = webHelper;
        _scheduleTaskService = scheduleTaskService;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/SpireProductImport/Configure";
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        // Settings
        var settings = new SpireProductImportSettings
        {
            FtpServer = "ftp.spire.co.uk",
            FtpUsername = "gamma",
            FtpPassword = "$VT7624tc",
            CsvFileName = "gamma.csv",
            XmlFileName = "gamma.xml",
            EnableAutomaticImport = false,
            ImportIntervalHours = 1,
            MarkupPercentage = 15.0m,
            AutoCreateCategories = true,
            UpdateExistingProducts = true,
            MarkMissingProductsAsDiscontinued = true,
            EnableDetailedLogging = true,
            DownloadProductImages = true,
            IsEnabled = false
        };

        await _settingService.SaveSettingAsync(settings);

        // Schedule Task
        await _scheduleTaskService.InsertTaskAsync(new ScheduleTask
        {
            Name = "Spire Product Import Task",
            Type = "Nop.Plugin.Misc.SpireProductImport.Services.SpireProductImportTask",
            Enabled = true,
            StopOnError = false,
            Seconds = 3600 // Run every hour (3600 seconds)
        });

        // Locales
        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Misc.SpireProductImport.Status"] = "Plugin Status",
            ["Plugins.Misc.SpireProductImport.FtpConfiguration"] = "FTP Configuration",
            ["Plugins.Misc.SpireProductImport.ImportSettings"] = "Import Settings",
            ["Plugins.Misc.SpireProductImport.Actions"] = "Actions",

            ["Plugins.Misc.SpireProductImport.FtpServer"] = "FTP Server",
            ["Plugins.Misc.SpireProductImport.FtpServer.Hint"] = "Enter the FTP server address (e.g., ftp.spire.co.uk).",

            ["Plugins.Misc.SpireProductImport.FtpUsername"] = "FTP Username",
            ["Plugins.Misc.SpireProductImport.FtpUsername.Hint"] = "Enter the FTP username for authentication.",

            ["Plugins.Misc.SpireProductImport.FtpPassword"] = "FTP Password",
            ["Plugins.Misc.SpireProductImport.FtpPassword.Hint"] = "Enter the FTP password for authentication.",

            ["Plugins.Misc.SpireProductImport.CsvFileName"] = "CSV File Name",
            ["Plugins.Misc.SpireProductImport.CsvFileName.Hint"] = "Enter the name of the CSV file to download (e.g., gamma.csv).",

            ["Plugins.Misc.SpireProductImport.XmlFileName"] = "XML File Name",
            ["Plugins.Misc.SpireProductImport.XmlFileName.Hint"] = "Enter the name of the XML file to download (optional, e.g., gamma.xml).",

            ["Plugins.Misc.SpireProductImport.IsEnabled"] = "Enable Plugin",
            ["Plugins.Misc.SpireProductImport.IsEnabled.Hint"] = "Check to enable the Spire Product Import plugin.",

            ["Plugins.Misc.SpireProductImport.EnableAutomaticImport"] = "Enable Automatic Import",
            ["Plugins.Misc.SpireProductImport.EnableAutomaticImport.Hint"] = "Check to enable automatic product imports at regular intervals.",

            ["Plugins.Misc.SpireProductImport.ImportIntervalHours"] = "Import Interval (Hours)",
            ["Plugins.Misc.SpireProductImport.ImportIntervalHours.Hint"] = "Set the number of hours between automatic imports (1-24).",

            ["Plugins.Misc.SpireProductImport.MarkupPercentage"] = "Markup Percentage",
            ["Plugins.Misc.SpireProductImport.MarkupPercentage.Hint"] = "Set the markup percentage to add to the cost price (default: 15%).",

            ["Plugins.Misc.SpireProductImport.AutoCreateCategories"] = "Auto-Create Categories",
            ["Plugins.Misc.SpireProductImport.AutoCreateCategories.Hint"] = "Check to automatically create product categories from the CSV data.",

            ["Plugins.Misc.SpireProductImport.UpdateExistingProducts"] = "Update Existing Products",
            ["Plugins.Misc.SpireProductImport.UpdateExistingProducts.Hint"] = "Check to update existing products with new data from imports.",

            ["Plugins.Misc.SpireProductImport.MarkMissingProductsAsDiscontinued"] = "Mark Missing Products as Discontinued",
            ["Plugins.Misc.SpireProductImport.MarkMissingProductsAsDiscontinued.Hint"] = "Check to mark products as discontinued if they're not found in the new import data.",

            ["Plugins.Misc.SpireProductImport.EnableDetailedLogging"] = "Enable Detailed Logging",
            ["Plugins.Misc.SpireProductImport.EnableDetailedLogging.Hint"] = "Check to enable detailed logging of import operations.",

            ["Plugins.Misc.SpireProductImport.DownloadProductImages"] = "Download Product Images",
            ["Plugins.Misc.SpireProductImport.DownloadProductImages.Hint"] = "Check to automatically download and save product images from URLs.",

            ["Plugins.Misc.SpireProductImport.LastImportDateTime"] = "Last Import",
            ["Plugins.Misc.SpireProductImport.LastSuccessfulImportDateTime"] = "Last Successful Import",
            ["Plugins.Misc.SpireProductImport.ImportStatus"] = "Status",

            ["Plugins.Misc.SpireProductImport.TestConnection"] = "Test Connection",
            ["Plugins.Misc.SpireProductImport.RunImportNow"] = "Run Import Now",
            ["Plugins.Misc.SpireProductImport.RunImportConfirm"] = "Are you sure you want to start the import process? This may take several minutes to complete."
        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        // Schedule Task
        var task = await _scheduleTaskService.GetTaskByTypeAsync("Nop.Plugin.Misc.SpireProductImport.Services.SpireProductImportTask");
        if (task != null)
            await _scheduleTaskService.DeleteTaskAsync(task);

        // Settings
        await _settingService.DeleteSettingAsync<SpireProductImportSettings>();

        // Locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.SpireProductImport");

        await base.UninstallAsync();
    }

    #endregion
}