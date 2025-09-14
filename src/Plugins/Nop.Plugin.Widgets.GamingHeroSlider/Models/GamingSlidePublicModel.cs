using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.GamingHeroSlider.Models;

/// <summary>
/// Represents a gaming slide model on the site
/// </summary>
public record GamingSlidePublicModel : BaseNopModel
{
    #region Properties

    public int PictureId { get; set; }
    public string PictureUrl { get; set; } = string.Empty;
    public string Badge { get; set; } = string.Empty;
    public string TitleText { get; set; } = string.Empty;
    public string TitleHighlight { get; set; } = string.Empty;
    public string LinkUrl { get; set; } = string.Empty;
    public string AltText { get; set; } = string.Empty;
    public string PrimaryButtonText { get; set; } = string.Empty;
    public string SecondaryButtonText { get; set; } = string.Empty;
    public string SecondaryButtonUrl { get; set; } = string.Empty;
    public string IconClass { get; set; } = string.Empty;
    public bool LazyLoading { get; set; }

    #endregion
}
