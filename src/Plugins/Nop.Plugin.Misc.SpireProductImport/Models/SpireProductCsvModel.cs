using CsvHelper.Configuration.Attributes;
using CsvHelper.Configuration;
using CsvHelper.TypeConversion;
using CsvHelper;
using System.Globalization;

namespace Nop.Plugin.Misc.SpireProductImport.Models;

/// <summary>
/// Represents a product from the Spire CSV file
/// </summary>
public class SpireProductCsvModel
{
    #region Properties

    [Name("Product Number")]
    public string ProductNumber { get; set; } = string.Empty;

    [Name("Manufacturer Number")]
    public string ManufacturerNumber { get; set; } = string.Empty;

    [Name("Category")]
    public string Category { get; set; } = string.Empty;

    [Name("Manufacturer")]
    public string Manufacturer { get; set; } = string.Empty;

    [Name("Description")]
    public string Description { get; set; } = string.Empty;

    [Name("Cost")]
    public decimal Cost { get; set; }

    [Name("Stock Level")]
    public int StockLevel { get; set; }

    [Name("Product Details")]
    public string ProductDetails { get; set; } = string.Empty;

    [Name("Specification")]
    public string Specification { get; set; } = string.Empty;

    [Name("EAN Number")]
    public string EanNumber { get; set; } = string.Empty;

    [Name("Thumbnail")]
    public string Thumbnail { get; set; } = string.Empty;

    [Name("Main Image")]
    public string MainImage { get; set; } = string.Empty;

    [Name("Weight")]
    [TypeConverter(typeof(WeightConverter))]
    public decimal Weight { get; set; }

    #endregion
}

/// <summary>
/// Custom converter for weight field that handles units like 'kg'
/// </summary>
public class WeightConverter : DefaultTypeConverter
{
    public override object ConvertFromString(string text, IReaderRow row, MemberMapData memberMapData)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 0m;

        // Remove common units and whitespace
        var cleanedText = text.Trim()
            .Replace(" kg", "")
            .Replace("kg", "")
            .Replace(" g", "")
            .Replace("g", "")
            .Replace(" lbs", "")
            .Replace("lbs", "")
            .Replace(" lb", "")
            .Replace("lb", "")
            .Trim();

        if (decimal.TryParse(cleanedText, NumberStyles.Any, CultureInfo.InvariantCulture, out var result))
            return result;

        // If parsing fails, return 0 instead of throwing exception
        return 0m;
    }
}