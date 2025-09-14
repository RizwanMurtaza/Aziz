namespace Nop.Plugin.Widgets.GamingHeroSlider.Domain;

/// <summary>
/// Represents a gaming slide item in the settings
/// </summary>
public class Slide
{
    #region Properties

    /// <summary>
    /// Picture identifier
    /// </summary>
    public int PictureId { get; set; }

    /// <summary>
    /// Badge text for gaming slide
    /// </summary>
    public string Badge { get; set; } = string.Empty;

    /// <summary>
    /// Main title text
    /// </summary>
    public string TitleText { get; set; } = string.Empty;

    /// <summary>
    /// Highlighted part of title
    /// </summary>
    public string TitleHighlight { get; set; } = string.Empty;

    /// <summary>
    /// Link URL for primary button
    /// </summary>
    public string LinkUrl { get; set; } = string.Empty;

    /// <summary>
    /// Image alternate text
    /// </summary>
    public string AltText { get; set; } = string.Empty;

    /// <summary>
    /// Primary button text
    /// </summary>
    public string PrimaryButtonText { get; set; } = string.Empty;

    /// <summary>
    /// Secondary button text
    /// </summary>
    public string SecondaryButtonText { get; set; } = string.Empty;

    /// <summary>
    /// Secondary button URL
    /// </summary>
    public string SecondaryButtonUrl { get; set; } = string.Empty;

    /// <summary>
    /// Icon CSS class (e.g., 'fas fa-microchip')
    /// </summary>
    public string IconClass { get; set; } = string.Empty;

    #endregion
}
