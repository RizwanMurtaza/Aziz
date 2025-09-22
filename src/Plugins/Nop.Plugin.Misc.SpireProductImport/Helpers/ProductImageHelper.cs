using Nop.Core;
using Nop.Core.Domain.Catalog;
using Nop.Plugin.Misc.SpireProductImport.Services;
using Nop.Services.Catalog;
using Nop.Services.Media;

namespace Nop.Plugin.Misc.SpireProductImport.Helpers;

public class ProductImageHelper
{
    private readonly IProductExternalImageService _productExternalImageService;
    private readonly IProductService _productService;
    private readonly IPictureService _pictureService;

    public ProductImageHelper(
        IProductExternalImageService productExternalImageService,
        IProductService productService,
        IPictureService pictureService)
    {
        _productExternalImageService = productExternalImageService;
        _productService = productService;
        _pictureService = pictureService;
    }

    public async Task<string> GetProductImageUrlAsync(int productId, int pictureSize = 0)
    {
        // First, check for external images
        var externalImages = await _productExternalImageService.GetProductExternalImagesAsync(productId);
        if (externalImages != null && !string.IsNullOrEmpty(externalImages.MainImageUrl))
        {
            return externalImages.MainImageUrl;
        }

        // Fallback to regular NopCommerce images
        var productPictures = await _productService.GetProductPicturesByProductIdAsync(productId);
        if (productPictures.Any())
        {
            var firstPicture = productPictures.OrderBy(x => x.DisplayOrder).First();
            return await _pictureService.GetPictureUrlAsync(firstPicture.PictureId, pictureSize);
        }

        // Return default "no image" URL
        return await _pictureService.GetDefaultPictureUrlAsync(pictureSize);
    }

    public async Task<List<string>> GetAllProductImageUrlsAsync(int productId)
    {
        var imageUrls = new List<string>();

        // Get external images
        var externalImages = await _productExternalImageService.GetProductExternalImagesAsync(productId);
        if (externalImages != null)
        {
            if (!string.IsNullOrEmpty(externalImages.MainImageUrl))
                imageUrls.Add(externalImages.MainImageUrl);

            if (!string.IsNullOrEmpty(externalImages.ThumbnailUrl) && externalImages.ThumbnailUrl != externalImages.MainImageUrl)
                imageUrls.Add(externalImages.ThumbnailUrl);
        }

        // Also get regular NopCommerce images if any exist
        var productPictures = await _productService.GetProductPicturesByProductIdAsync(productId);
        foreach (var pp in productPictures.OrderBy(x => x.DisplayOrder))
        {
            var url = await _pictureService.GetPictureUrlAsync(pp.PictureId);
            if (!string.IsNullOrEmpty(url))
                imageUrls.Add(url);
        }

        return imageUrls;
    }
}