using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Nop.Plugin.Misc.SpireProductImport.Services;
using Nop.Services.Logging;
using Nop.Web.Models.Catalog;
using System.Linq;
using System.Reflection;

namespace Nop.Plugin.Misc.SpireProductImport.Filters;

public class ExternalImageFilter : IAsyncActionFilter
{
    private readonly IProductExternalImageService _productExternalImageService;
    private readonly ILogger _logger;

    public ExternalImageFilter(IProductExternalImageService productExternalImageService, ILogger logger)
    {
        _productExternalImageService = productExternalImageService;
        _logger = logger;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        // Execute the action
        var resultContext = await next();

        await _logger.InformationAsync($"ExternalImageFilter: Action executed for {context.Controller.GetType().Name}.{context.ActionDescriptor.DisplayName}");

        if (resultContext.Result is ViewResult viewResult)
        {
            await _logger.InformationAsync($"ExternalImageFilter: ViewResult found, Model type: {viewResult.Model?.GetType().Name ?? "null"}");

            if (viewResult.Model is ProductDetailsModel productDetailsModel)
            {
                await _logger.InformationAsync($"ExternalImageFilter: Found ProductDetailsModel for product {productDetailsModel.Id}");
                await ProcessProductDetailsModel(productDetailsModel);
            }
            else if (viewResult.Model is CatalogProductsModel catalogModel)
            {
                await _logger.InformationAsync($"ExternalImageFilter: Found CatalogProductsModel with {catalogModel.Products.Count} products");
                foreach (var product in catalogModel.Products)
                {
                    await ProcessProductOverviewModel(product);
                }
            }
            else if (viewResult.ViewData.Model is CatalogProductsModel catalogModelFromViewData)
            {
                await _logger.InformationAsync($"ExternalImageFilter: Found CatalogProductsModel in ViewData with {catalogModelFromViewData.Products.Count} products");
                foreach (var product in catalogModelFromViewData.Products)
                {
                    await ProcessProductOverviewModel(product);
                }
            }
            else
            {
                await _logger.InformationAsync($"ExternalImageFilter: Unhandled model type: {viewResult.Model?.GetType().FullName ?? "null"}");

                // Let's also check ViewData for any models
                if (viewResult.ViewData.Model != null)
                {
                    await _logger.InformationAsync($"ExternalImageFilter: ViewData Model type: {viewResult.ViewData.Model.GetType().FullName}");
                }

                // Check if it has products property using reflection
                var modelType = viewResult.Model?.GetType();
                if (modelType != null)
                {
                    var productsProperty = modelType.GetProperty("Products");
                    if (productsProperty != null)
                    {
                        var products = productsProperty.GetValue(viewResult.Model) as IEnumerable<ProductOverviewModel>;
                        if (products != null)
                        {
                            await _logger.InformationAsync($"ExternalImageFilter: Found Products property with {products.Count()} products");
                            foreach (var product in products)
                            {
                                await ProcessProductOverviewModel(product);
                            }
                        }
                    }
                }
            }
        }
    }

    private async Task ProcessProductDetailsModel(ProductDetailsModel model)
    {
        try
        {
            var externalImages = await _productExternalImageService.GetProductExternalImagesAsync(model.Id);
            await _logger.InformationAsync($"ExternalImageFilter: Retrieved external images for product details {model.Id}: MainImage={externalImages?.MainImageUrl}");

            if (externalImages != null && !string.IsNullOrWhiteSpace(externalImages.MainImageUrl))
            {
                // Replace the default picture model with external image URL
                if (model.DefaultPictureModel != null)
                {
                    await _logger.InformationAsync($"ExternalImageFilter: Replacing ProductDetails default image URL from {model.DefaultPictureModel.ImageUrl} to {externalImages.MainImageUrl}");

                    model.DefaultPictureModel.ImageUrl = externalImages.MainImageUrl;
                    model.DefaultPictureModel.FullSizeImageUrl = externalImages.MainImageUrl;
                }

                // Also update the picture models collection
                if (model.PictureModels?.Any() == true)
                {
                    var firstPicture = model.PictureModels.First();
                    await _logger.InformationAsync($"ExternalImageFilter: Replacing ProductDetails picture model URL from {firstPicture.ImageUrl} to {externalImages.MainImageUrl}");

                    firstPicture.ImageUrl = externalImages.MainImageUrl;
                    firstPicture.FullSizeImageUrl = externalImages.MainImageUrl;

                    // Also update thumbnail if available
                    if (!string.IsNullOrWhiteSpace(externalImages.ThumbnailUrl))
                    {
                        firstPicture.ThumbImageUrl = externalImages.ThumbnailUrl;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ExternalImageFilter: Error in ProductDetailsModel handler for product {model.Id}: {ex.Message}", ex);
        }
    }

    private async Task ProcessProductOverviewModel(ProductOverviewModel model)
    {
        try
        {
            var externalImages = await _productExternalImageService.GetProductExternalImagesAsync(model.Id);
            await _logger.InformationAsync($"ExternalImageFilter: Retrieved external images for product {model.Id}: MainImage={externalImages?.MainImageUrl}");

            if (externalImages != null && !string.IsNullOrWhiteSpace(externalImages.MainImageUrl))
            {
                // Replace the default picture model with external image URL
                if (model.PictureModels?.Any() == true)
                {
                    var firstPicture = model.PictureModels.First();
                    await _logger.InformationAsync($"ExternalImageFilter: Replacing ProductOverview image URL from {firstPicture.ImageUrl} to {externalImages.MainImageUrl}");

                    firstPicture.ImageUrl = externalImages.MainImageUrl;
                    firstPicture.FullSizeImageUrl = externalImages.MainImageUrl;

                    // Also update thumbnail if available
                    if (!string.IsNullOrWhiteSpace(externalImages.ThumbnailUrl))
                    {
                        firstPicture.ThumbImageUrl = externalImages.ThumbnailUrl;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ExternalImageFilter: Error in ProductOverviewModel handler for product {model.Id}: {ex.Message}", ex);
        }
    }
}