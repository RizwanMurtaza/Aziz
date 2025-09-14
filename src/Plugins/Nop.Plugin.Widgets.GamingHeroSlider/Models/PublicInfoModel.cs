using Nop.Web.Framework.Models;

namespace Nop.Plugin.Widgets.GamingHeroSlider.Models;

/// <summary>
/// Represents Gaming Hero Slider public model
/// </summary>
public record PublicInfoModel : BaseNopModel
{
    #region Properties

    public bool ShowNavigation { get; set; }
    public bool ShowPagination { get; set; }
    public bool Autoplay { get; set; }
    public int AutoplayDelay { get; set; }
    public List<GamingSlideModel> Slides { get; set; } = new();

    #endregion
}