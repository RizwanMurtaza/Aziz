using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Nop.Core;
using Nop.Core.Caching;
using Nop.Plugin.Widgets.GamingHeroSlider.Domain;
using Nop.Plugin.Widgets.GamingHeroSlider.Infrastructure.Cache;
using Nop.Plugin.Widgets.GamingHeroSlider.Models;
using Nop.Services.Configuration;
using Nop.Services.Media;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Widgets.GamingHeroSlider.Components;

public class GamingHeroSliderViewComponent : NopViewComponent
{
    #region Fields

    protected readonly IPictureService _pictureService;
    protected readonly IStaticCacheManager _staticCacheManager;
    protected readonly ISettingService _settingService;
    protected readonly IStoreContext _storeContext;
    protected readonly IWebHelper _webHelper;

    #endregion

    #region Ctor

    public GamingHeroSliderViewComponent(IPictureService pictureService,
    IStaticCacheManager staticCacheManager,
    ISettingService settingService,
    IStoreContext storeContext,
    IWebHelper webHelper)
    {
        _pictureService = pictureService;
        _staticCacheManager = staticCacheManager;
        _settingService = settingService;
        _storeContext = storeContext;
        _webHelper = webHelper;
    }

    #endregion

    #region Utilities

    /// <returns>A task that represents the asynchronous operation</returns>
    private async Task<string> GetPictureUrlAsync(int pictureId)
    {
        if (pictureId == 0)
            return string.Empty;

        var cacheKey = _staticCacheManager.PrepareKeyForDefaultCache(ModelCacheEventConsumer.PictureUrlModelKey,
            pictureId, _webHelper.IsCurrentConnectionSecured());

        return await _staticCacheManager.GetAsync(cacheKey, async () =>
        {
            //little hack here. nulls aren't cacheable so set it to ""
            var url = await _pictureService.GetPictureUrlAsync(pictureId, showDefaultPicture: false) ?? "";
            return url;
        });
    }

    #endregion

    #region Methods

    /// <returns>A task that represents the asynchronous operation</returns>
    public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
    {
        var store = await _storeContext.GetCurrentStoreAsync();
        var sliderSettings = await _settingService.LoadSettingAsync<GamingHeroSliderSettings>(store.Id);

        if (string.IsNullOrEmpty(sliderSettings.Slides))
            return Content("");

        var model = new PublicInfoModel
        {
            ShowNavigation = sliderSettings.ShowNavigation,
            ShowPagination = sliderSettings.ShowPagination,
            Autoplay = sliderSettings.Autoplay,
            AutoplayDelay = sliderSettings.AutoplayDelay,
        };

        var slides = JsonConvert.DeserializeObject<List<Slide>>(sliderSettings.Slides);
        foreach (var slide in slides)
        {
            var picUrl = await GetPictureUrlAsync(slide.PictureId);
            if (string.IsNullOrEmpty(picUrl))
                continue;

            model.Slides.Add(new()
            {
                PictureUrl = picUrl,
                TitleText = slide.TitleText,
                TitleHighlight = slide.TitleHighlight,
                Badge = slide.Badge,
                LinkUrl = slide.LinkUrl,
                AltText = slide.AltText,
                PrimaryButtonText = slide.PrimaryButtonText,
                SecondaryButtonText = slide.SecondaryButtonText,
                SecondaryButtonUrl = slide.SecondaryButtonUrl,
                IconClass = slide.IconClass,
                LazyLoading = sliderSettings.LazyLoading
            });
        }

        if (!model.Slides.Any())
            return Content("");

        return View("~/Plugins/Widgets.GamingHeroSlider/Views/PublicInfo.cshtml", model);
    }

    #endregion
}