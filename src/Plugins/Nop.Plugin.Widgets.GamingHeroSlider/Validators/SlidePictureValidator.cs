using FluentValidation;
using Nop.Plugin.Widgets.GamingHeroSlider.Models;
using Nop.Services.Localization;
using Nop.Web.Framework.Validators;

namespace Nop.Plugin.Widgets.GamingHeroSlider.Validators;

/// <summary>
/// Represents gaming slide picture model validator
/// </summary>
public class GamingSlidePictureValidator : BaseNopValidator<GamingSlidePictureModel>
{
    #region Ctor

    public GamingSlidePictureValidator(ILocalizationService localizationService)
    {
        RuleFor(model => model.PictureId)
            .GreaterThan(0)
            .WithMessageAwait(localizationService.GetResourceAsync("Plugins.Widgets.GamingHeroSlider.Picture.Required"));
    }

    #endregion
}
