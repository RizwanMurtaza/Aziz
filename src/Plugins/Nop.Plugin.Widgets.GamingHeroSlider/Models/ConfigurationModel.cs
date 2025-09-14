using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Widgets.GamingHeroSlider.Models;

/// <summary>
/// Represents Gaming Hero Slider configuration model
/// </summary>
public record ConfigurationModel : BaseNopModel
{
    #region Properties

    public int ActiveStoreScopeConfiguration { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.ShowNavigation")]
    public bool ShowNavigation { get; set; }
    public bool ShowNavigation_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.ShowPagination")]
    public bool ShowPagination { get; set; }
    public bool ShowPagination_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.Autoplay")]
    public bool Autoplay { get; set; }
    public bool Autoplay_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.AutoplayDelay")]
    public int AutoplayDelay { get; set; }
    public bool AutoplayDelay_OverrideForStore { get; set; }

    [NopResourceDisplayName("Plugins.Widgets.GamingHeroSlider.LazyLoading")]
    public bool LazyLoading { get; set; }
    public bool LazyLoading_OverrideForStore { get; set; }

    public GamingSlidesSearchModel SlidesSearchModel { get; set; } = new();
    public GamingSlidePictureModel AddSlideModel { get; set; } = new();

    #endregion
}