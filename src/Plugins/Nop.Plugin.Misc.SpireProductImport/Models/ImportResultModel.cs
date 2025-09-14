namespace Nop.Plugin.Misc.SpireProductImport.Models;

/// <summary>
/// Represents the result of a product import operation
/// </summary>
public class ImportResultModel
{
    #region Properties

    /// <summary>
    /// Gets or sets a value indicating whether the import was successful
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Gets or sets the total number of products processed
    /// </summary>
    public int TotalProcessed { get; set; }

    /// <summary>
    /// Gets or sets the number of products successfully imported/updated
    /// </summary>
    public int SuccessfullyImported { get; set; }

    /// <summary>
    /// Gets or sets the number of new products created
    /// </summary>
    public int NewProductsCreated { get; set; }

    /// <summary>
    /// Gets or sets the number of existing products updated
    /// </summary>
    public int ExistingProductsUpdated { get; set; }

    /// <summary>
    /// Gets or sets the number of products marked as discontinued
    /// </summary>
    public int ProductsMarkedDiscontinued { get; set; }

    /// <summary>
    /// Gets or sets the number of categories created
    /// </summary>
    public int CategoriesCreated { get; set; }

    /// <summary>
    /// Gets or sets the number of manufacturers created
    /// </summary>
    public int ManufacturersCreated { get; set; }

    /// <summary>
    /// Gets or sets the number of errors encountered
    /// </summary>
    public int ErrorCount { get; set; }

    /// <summary>
    /// Gets or sets the list of error messages
    /// </summary>
    public List<string> ErrorMessages { get; set; } = new();

    /// <summary>
    /// Gets or sets the import start time
    /// </summary>
    public DateTime ImportStartTime { get; set; }

    /// <summary>
    /// Gets or sets the import end time
    /// </summary>
    public DateTime ImportEndTime { get; set; }

    /// <summary>
    /// Gets the total import duration
    /// </summary>
    public TimeSpan ImportDuration => ImportEndTime - ImportStartTime;

    /// <summary>
    /// Gets the import summary message
    /// </summary>
    public string SummaryMessage
    {
        get
        {
            if (!IsSuccess)
                return $"Import failed with {ErrorCount} errors";

            return $"Import completed successfully. Processed {TotalProcessed} products, " +
                   $"Created {NewProductsCreated} new products, " +
                   $"Updated {ExistingProductsUpdated} existing products, " +
                   $"Created {CategoriesCreated} categories, " +
                   $"Created {ManufacturersCreated} manufacturers. " +
                   $"Duration: {ImportDuration:hh\\:mm\\:ss}";
        }
    }

    #endregion
}