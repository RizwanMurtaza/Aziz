using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.SpireProductImport.Models;

/// <summary>
/// Represents Spire Product Import configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Properties

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.FtpServer")]
    public string FtpServer { get; set; } = string.Empty;
    public bool FtpServer_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.FtpUsername")]
    public string FtpUsername { get; set; } = string.Empty;
    public bool FtpUsername_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.FtpPassword")]
    [DataType(DataType.Password)]
    public string FtpPassword { get; set; } = string.Empty;
    public bool FtpPassword_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.CsvFileName")]
    public string CsvFileName { get; set; } = string.Empty;
    public bool CsvFileName_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.XmlFileName")]
    public string XmlFileName { get; set; } = string.Empty;
    public bool XmlFileName_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.IsEnabled")]
    public bool IsEnabled { get; set; }
    public bool IsEnabled_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.EnableAutomaticImport")]
    public bool EnableAutomaticImport { get; set; }
    public bool EnableAutomaticImport_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.ImportIntervalHours")]
    [Range(1, 24)]
    public int ImportIntervalHours { get; set; }
    public bool ImportIntervalHours_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.MarkupPercentage")]
    [Range(0, 1000)]
    public decimal MarkupPercentage { get; set; }
    public bool MarkupPercentage_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.AutoCreateCategories")]
    public bool AutoCreateCategories { get; set; }
    public bool AutoCreateCategories_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.UpdateExistingProducts")]
    public bool UpdateExistingProducts { get; set; }
    public bool UpdateExistingProducts_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.MarkMissingProductsAsDiscontinued")]
    public bool MarkMissingProductsAsDiscontinued { get; set; }
    public bool MarkMissingProductsAsDiscontinued_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.EnableDetailedLogging")]
    public bool EnableDetailedLogging { get; set; }
    public bool EnableDetailedLogging_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.DownloadProductImages")]
    public bool DownloadProductImages { get; set; }
    public bool DownloadProductImages_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.LastImportDateTime")]
    public DateTime? LastImportDateTime { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.LastSuccessfulImportDateTime")]
    public DateTime? LastSuccessfulImportDateTime { get; set; }

    [NopResourceDisplayName("Plugins.Misc.SpireProductImport.ImportStatus")]
    public string ImportStatus { get; set; } = string.Empty;

    #endregion
}