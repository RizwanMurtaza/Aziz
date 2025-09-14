using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.SpireProductImport;

/// <summary>
/// Represents Spire Product Import plugin settings
/// </summary>
public class SpireProductImportSettings : ISettings
{
    #region Properties

    /// <summary>
    /// Gets or sets the FTP server address
    /// </summary>
    public string FtpServer { get; set; } = "ftp.spire.co.uk";

    /// <summary>
    /// Gets or sets the FTP username
    /// </summary>
    public string FtpUsername { get; set; } = "gamma";

    /// <summary>
    /// Gets or sets the FTP password
    /// </summary>
    public string FtpPassword { get; set; } = "$VT7624tc";

    /// <summary>
    /// Gets or sets the CSV filename
    /// </summary>
    public string CsvFileName { get; set; } = "gamma.csv";

    /// <summary>
    /// Gets or sets the XML filename (optional)
    /// </summary>
    public string XmlFileName { get; set; } = "gamma.xml";

    /// <summary>
    /// Gets or sets a value indicating whether to enable automatic imports
    /// </summary>
    public bool EnableAutomaticImport { get; set; } = true;

    /// <summary>
    /// Gets or sets the import interval in hours
    /// </summary>
    public int ImportIntervalHours { get; set; } = 1;

    /// <summary>
    /// Gets or sets the markup percentage to add to cost price
    /// </summary>
    public decimal MarkupPercentage { get; set; } = 15.0m;

    /// <summary>
    /// Gets or sets a value indicating whether to create categories automatically
    /// </summary>
    public bool AutoCreateCategories { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to update existing products
    /// </summary>
    public bool UpdateExistingProducts { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to mark missing products as discontinued
    /// </summary>
    public bool MarkMissingProductsAsDiscontinued { get; set; } = true;

    /// <summary>
    /// Gets or sets the last import date and time
    /// </summary>
    public DateTime? LastImportDateTime { get; set; }

    /// <summary>
    /// Gets or sets the last successful import date and time
    /// </summary>
    public DateTime? LastSuccessfulImportDateTime { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether to log detailed information
    /// </summary>
    public bool EnableDetailedLogging { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether to download images
    /// </summary>
    public bool DownloadProductImages { get; set; } = true;

    /// <summary>
    /// Gets or sets a value indicating whether the plugin is enabled
    /// </summary>
    public bool IsEnabled { get; set; } = false;

    #endregion
}