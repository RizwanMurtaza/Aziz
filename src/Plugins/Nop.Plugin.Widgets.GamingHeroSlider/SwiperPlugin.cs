using System.Text.Json;
using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Infrastructure;
using Nop.Plugin.Widgets.GamingHeroSlider.Components;
using Nop.Plugin.Widgets.GamingHeroSlider.Domain;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Media;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;

namespace Nop.Plugin.Widgets.GamingHeroSlider;

/// <summary>
/// Represents Gaming Hero Slider widget
/// </summary>
public class GamingHeroSliderPlugin : BasePlugin, IWidgetPlugin
{
    #region Fields

    protected readonly ILocalizationService _localizationService;
    protected readonly INopFileProvider _fileProvider;
    protected readonly IPictureService _pictureService;
    protected readonly ISettingService _settingService;
    protected readonly IWebHelper _webHelper;
    protected readonly WidgetSettings _widgetSettings;

    #endregion

    #region Ctor

    public GamingHeroSliderPlugin(ILocalizationService localizationService,
        INopFileProvider fileProvider,
        IPictureService pictureService,
        ISettingService settingService,
        IWebHelper webHelper,
        WidgetSettings widgetSettings)
    {
        _localizationService = localizationService;
        _fileProvider = fileProvider;
        _pictureService = pictureService;
        _settingService = settingService;
        _webHelper = webHelper;
        _widgetSettings = widgetSettings;
    }

    #endregion

    #region Methods

    /// <summary>
    /// Gets widget zones where this widget should be rendered
    /// </summary>
    /// <returns>
    /// A task that represents the asynchronous operation
    /// The task result contains the widget zones
    /// </returns>
    public Task<IList<string>> GetWidgetZonesAsync()
    {
        return Task.FromResult<IList<string>>(new List<string> { PublicWidgetZones.HomepageTop });
    }

    /// <summary>
    /// Gets a configuration page URL
    /// </summary>
    public override string GetConfigurationPageUrl()
    {
        return _webHelper.GetStoreLocation() + "Admin/GamingHeroSlider/Configure";
    }

    /// <summary>
    /// Gets a name of a view component for displaying widget
    /// </summary>
    /// <param name="widgetZone">Name of the widget zone</param>
    /// <returns>View component name</returns>
    public Type GetWidgetViewComponent(string widgetZone)
    {
        return typeof(GamingHeroSliderViewComponent);
    }

    /// <summary>
    /// Install plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task InstallAsync()
    {
        //pictures
        var sampleImagesPath = _fileProvider.MapPath("~/Plugins/Widgets.GamingHeroSlider/Content/sample-images/");

        //settings

        var slides = new List<Slide>
        {
            new()
            {
                PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "banner_01.webp")), MimeTypes.ImageWebp, "banner_1")).Id,
                TitleText = string.Empty,
                AltText = string.Empty,
                LinkUrl = _webHelper.GetStoreLocation(),
            },
            new()
            {
                PictureId = (await _pictureService.InsertPictureAsync(await _fileProvider.ReadAllBytesAsync(_fileProvider.Combine(sampleImagesPath, "banner_02.webp")), MimeTypes.ImageWebp, "banner_2")).Id,
                TitleText = string.Empty,
                AltText = string.Empty,
                LinkUrl = _webHelper.GetStoreLocation(),
            }
        };

        var settings = new GamingHeroSliderSettings
        {
            ShowNavigation = false,
            ShowPagination = true,
            Autoplay = true,
            AutoplayDelay = 3000,
            LazyLoading = true,
            Slides = JsonSerializer.Serialize(slides)
        };
        await _settingService.SaveSettingAsync(settings);

        if (!_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Add(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
        {
            ["Plugins.Widgets.GamingHeroSlider.Slide"] = "Gaming Slide",
            ["Plugins.Widgets.GamingHeroSlider.SlideList"] = "Add new gaming slide",
            ["Plugins.Widgets.GamingHeroSlider.Slide.Add"] = "Add",
            ["Plugins.Widgets.GamingHeroSlider.Settings"] = "Gaming Hero Slider Settings",
            ["Plugins.Widgets.GamingHeroSlider.Picture"] = "Picture",
            ["Plugins.Widgets.GamingHeroSlider.Picture.Hint"] = "Upload picture for gaming slide.",
            ["Plugins.Widgets.GamingHeroSlider.Picture.Required"] = "Picture is required",
            ["Plugins.Widgets.GamingHeroSlider.Badge"] = "Badge",
            ["Plugins.Widgets.GamingHeroSlider.Badge.Hint"] = "Enter badge text (e.g., 'New Arrivals', 'Gaming PC').",
            ["Plugins.Widgets.GamingHeroSlider.TitleText"] = "Title",
            ["Plugins.Widgets.GamingHeroSlider.TitleText.Hint"] = "Enter main title text.",
            ["Plugins.Widgets.GamingHeroSlider.TitleHighlight"] = "Title Highlight",
            ["Plugins.Widgets.GamingHeroSlider.TitleHighlight.Hint"] = "Enter highlighted part of title.",
            ["Plugins.Widgets.GamingHeroSlider.LinkUrl"] = "Primary Button URL",
            ["Plugins.Widgets.GamingHeroSlider.LinkUrl.Hint"] = "Enter URL for primary button.",
            ["Plugins.Widgets.GamingHeroSlider.AltText"] = "Image alternate text",
            ["Plugins.Widgets.GamingHeroSlider.AltText.Hint"] = "Enter alternate text for accessibility.",
            ["Plugins.Widgets.GamingHeroSlider.PrimaryButtonText"] = "Primary Button Text",
            ["Plugins.Widgets.GamingHeroSlider.PrimaryButtonText.Hint"] = "Enter primary button text.",
            ["Plugins.Widgets.GamingHeroSlider.SecondaryButtonText"] = "Secondary Button Text",
            ["Plugins.Widgets.GamingHeroSlider.SecondaryButtonText.Hint"] = "Enter secondary button text.",
            ["Plugins.Widgets.GamingHeroSlider.SecondaryButtonUrl"] = "Secondary Button URL",
            ["Plugins.Widgets.GamingHeroSlider.SecondaryButtonUrl.Hint"] = "Enter URL for secondary button.",
            ["Plugins.Widgets.GamingHeroSlider.IconClass"] = "Icon Class",
            ["Plugins.Widgets.GamingHeroSlider.IconClass.Hint"] = "Enter Font Awesome icon class (e.g., 'fas fa-microchip').",
            ["Plugins.Widgets.GamingHeroSlider.Autoplay"] = "Autoplay",
            ["Plugins.Widgets.GamingHeroSlider.Autoplay.Hint"] = "Check to enable autoplay.",
            ["Plugins.Widgets.GamingHeroSlider.LazyLoading"] = "Lazy loading",
            ["Plugins.Widgets.GamingHeroSlider.LazyLoading.Hint"] = "Check to enable lazy loading of pictures.",
            ["Plugins.Widgets.GamingHeroSlider.AutoplayDelay"] = "Delay",
            ["Plugins.Widgets.GamingHeroSlider.AutoplayDelay.Hint"] = "Delay between transitions (in ms).",
            ["Plugins.Widgets.GamingHeroSlider.ShowNavigation"] = "Show navigation arrows",
            ["Plugins.Widgets.GamingHeroSlider.ShowNavigation.Hint"] = "Check to display navigation arrows.",
            ["Plugins.Widgets.GamingHeroSlider.ShowPagination"] = "Show pagination",
            ["Plugins.Widgets.GamingHeroSlider.ShowPagination.Hint"] = "Check to display pagination dots.",

        });

        await base.InstallAsync();
    }

    /// <summary>
    /// Uninstall plugin
    /// </summary>
    /// <returns>A task that represents the asynchronous operation</returns>
    public override async Task UninstallAsync()
    {
        //settings
        await _settingService.DeleteSettingAsync<GamingHeroSliderSettings>();
        if (_widgetSettings.ActiveWidgetSystemNames.Contains(PluginDescriptor.SystemName))
        {
            _widgetSettings.ActiveWidgetSystemNames.Remove(PluginDescriptor.SystemName);
            await _settingService.SaveSettingAsync(_widgetSettings);
        }

        //locales
        await _localizationService.DeleteLocaleResourcesAsync("Plugins.Widgets.GamingHeroSlider");

        await base.UninstallAsync();
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gets a value indicating whether to hide this plugin on the widget list page in the admin area
    /// </summary>
    public bool HideInWidgetList => false;

    #endregion
}