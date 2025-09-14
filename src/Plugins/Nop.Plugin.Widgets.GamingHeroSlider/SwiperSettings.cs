using Nop.Core.Configuration;

namespace Nop.Plugin.Widgets.GamingHeroSlider;

/// <summary>
/// Represents Gaming Hero Slider plugin settings
/// </summary>
public class GamingHeroSliderSettings : ISettings
{
    #region Properties

    public bool ShowNavigation { get; set; }
    public bool ShowPagination { get; set; }
    public bool Autoplay { get; set; }
    public int AutoplayDelay { get; set; }
    public bool LazyLoading { get; set; }
    public string Slides { get; set; }

    #endregion
}