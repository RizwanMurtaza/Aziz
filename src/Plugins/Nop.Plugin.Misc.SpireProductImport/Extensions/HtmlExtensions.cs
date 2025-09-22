using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.DependencyInjection;
using Nop.Plugin.Misc.SpireProductImport.Services;

namespace Nop.Plugin.Misc.SpireProductImport.Extensions;

public static class HtmlExtensions
{
    /// <summary>
    /// Gets the external image URL for a product, or returns the default image URL if none exists
    /// </summary>
    public static async Task<string> GetProductExternalImageUrlAsync(this IHtmlHelper htmlHelper, int productId, string defaultImageUrl = null)
    {
        try
        {
            var externalImageService = htmlHelper.ViewContext.HttpContext.RequestServices.GetService<IProductExternalImageService>();
            if (externalImageService != null)
            {
                var externalImages = await externalImageService.GetProductExternalImagesAsync(productId);
                if (externalImages != null && !string.IsNullOrWhiteSpace(externalImages.MainImageUrl))
                {
                    return externalImages.MainImageUrl;
                }
            }
        }
        catch
        {
            // Fall through to default
        }

        return defaultImageUrl ?? string.Empty;
    }

    /// <summary>
    /// Renders an external image tag with fallback to default image
    /// </summary>
    public static async Task<IHtmlContent> RenderProductExternalImageAsync(this IHtmlHelper htmlHelper, int productId, string defaultImageUrl, string altText = "", string cssClass = "", string title = "")
    {
        var imageUrl = await GetProductExternalImageUrlAsync(htmlHelper, productId, defaultImageUrl);

        var imgTag = new TagBuilder("img");
        imgTag.Attributes["src"] = imageUrl;

        if (!string.IsNullOrEmpty(altText))
            imgTag.Attributes["alt"] = altText;

        if (!string.IsNullOrEmpty(title))
            imgTag.Attributes["title"] = title;

        if (!string.IsNullOrEmpty(cssClass))
            imgTag.Attributes["class"] = cssClass;

        return imgTag;
    }
}