using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.GamingHeroSlider.Models;

/// <summary>
/// Represents Gaming Hero Slide model
/// </summary>
public record GamingSlideModel : BaseNopEntityModel
{
    #region Properties

    /// <summary>
    /// Gets or sets the picture identifier
    /// </summary>
    public int PictureId { get; set; }

    /// <summary>
    /// Gets or sets the picture URL
    /// </summary>
    public string PictureUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slide badge text (e.g., "New Arrivals", "Limited Time")
    /// </summary>
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the main title text
    /// </summary>
    public string TitleText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the highlighted part of the title
    /// </summary>
    public string TitleHighlight { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the slide description
    /// </summary>
    public string AltText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary button text
    /// </summary>
    public string PrimaryButtonText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the primary button URL
    /// </summary>
    public string LinkUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secondary button text
    /// </summary>
    public string SecondaryButtonText { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the secondary button URL
    /// </summary>
    public string SecondaryButtonUrl { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the icon CSS class (e.g., "fas fa-microchip")
    /// </summary>
    public string IconClass { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets lazy loading
    /// </summary>
    public bool LazyLoading { get; set; }

    /// <summary>
    /// Gets or sets the display order
    /// </summary>
    public int DisplayOrder { get; set; }

    #endregion
}