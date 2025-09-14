using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.GamingHeroSlider.Models;

/// <summary>
/// Represents Gaming Hero Slide picture model
/// </summary>
public record GamingSlidePictureModel : BaseNopModel
{
    #region Properties

    [UIHint("Picture")]
    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.Picture")]
    public int PictureId { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.Badge")]
    public string Badge { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.TitleText")]
    public string TitleText { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.TitleHighlight")]
    public string TitleHighlight { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.LinkUrl")]
    public string LinkUrl { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.AltText")]
    public string AltText { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.PrimaryButtonText")]
    public string PrimaryButtonText { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.SecondaryButtonText")]
    public string SecondaryButtonText { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.SecondaryButtonUrl")]
    public string SecondaryButtonUrl { get; set; } = string.Empty;

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.IconClass")]
    public string IconClass { get; set; } = string.Empty;

    #endregion
}
