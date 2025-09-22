using System.Linq;
using Nop.Plugin.Misc.SpireProductImport.Services;
using Nop.Services.Events;
using Nop.Services.Logging;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Models;
using Nop.Web.Models.Catalog;

namespace Nop.Plugin.Misc.SpireProductImport.Events;

/// <summary>
/// Event consumer to inject external image URLs into product models
/// </summary>
public class ProductModelEventConsumer : IConsumer<ModelPreparedEvent<BaseNopModel>>
{
    private readonly IProductExternalImageService _productExternalImageService;
    private readonly ILogger _logger;

    public ProductModelEventConsumer(IProductExternalImageService productExternalImageService, ILogger logger)
    {
        _productExternalImageService = productExternalImageService;
        _logger = logger;

        // Log that the event consumer was created
        _ = Task.Run(async () => await _logger.InformationAsync("ProductModelEventConsumer: Event consumer instantiated successfully"));
    }

    /// <summary>
    /// Handle model prepared event for all models - filter for product models
    /// </summary>
    public async Task HandleEventAsync(ModelPreparedEvent<BaseNopModel> eventMessage)
    {
        await _logger.InformationAsync($"ProductModelEventConsumer: BaseNopModel event triggered for model type {eventMessage?.Model?.GetType().FullName}");

        if (eventMessage?.Model == null)
            return;

        try
        {
            // Handle ProductOverviewModel (category pages, search results, etc.)
            if (eventMessage.Model is ProductOverviewModel productOverviewModel)
            {
                await _logger.InformationAsync($"ProductModelEventConsumer: Processing ProductOverviewModel for product ID {productOverviewModel.Id}");
                await ProcessProductOverviewModel(productOverviewModel);
            }
            // Handle ProductDetailsModel (individual product pages)
            else if (eventMessage.Model is ProductDetailsModel productDetailsModel)
            {
                await _logger.InformationAsync($"ProductModelEventConsumer: Processing ProductDetailsModel for product ID {productDetailsModel.Id}");
                await ProcessProductDetailsModel(productDetailsModel);
            }
            // Handle CatalogProductsModel (category page main model)
            else if (eventMessage.Model is CatalogProductsModel catalogModel)
            {
                await _logger.InformationAsync($"ProductModelEventConsumer: Processing CatalogProductsModel with {catalogModel.Products?.Count ?? 0} products");
                await ProcessCatalogProductsModel(catalogModel);
            }
            // Handle CategoryModel (category page model)
            else if (eventMessage.Model is CategoryModel categoryModel)
            {
                await _logger.InformationAsync($"ProductModelEventConsumer: Found CategoryModel - processing products");
                await ProcessCategoryModel(categoryModel);
            }
            else
            {
                // Log unhandled model types to help identify what we're missing
                var modelType = eventMessage.Model.GetType().FullName;
                if (modelType.Contains("Product") || modelType.Contains("Catalog"))
                {
                    await _logger.InformationAsync($"ProductModelEventConsumer: Unhandled product-related model type: {modelType}");
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ProductModelEventConsumer: Error in model handler: {ex.Message}", ex);
        }
    }

    private async Task ProcessProductOverviewModel(ProductOverviewModel model)
    {
        try
        {
            var externalImages = await _productExternalImageService.GetProductExternalImagesAsync(model.Id);
            await _logger.InformationAsync($"ProductModelEventConsumer: Retrieved external images for product {model.Id}: MainImage={externalImages?.MainImageUrl}");

            if (externalImages != null && !string.IsNullOrWhiteSpace(externalImages.MainImageUrl))
            {
                // Replace the default picture model with external image URL
                if (model.PictureModels?.Any() == true)
                {
                    var firstPicture = model.PictureModels.First();
                    await _logger.InformationAsync($"ProductModelEventConsumer: Replacing ProductOverview image URL from {firstPicture.ImageUrl} to {externalImages.MainImageUrl}");

                    firstPicture.ImageUrl = externalImages.MainImageUrl;
                    firstPicture.FullSizeImageUrl = externalImages.MainImageUrl;

                    // Update thumbnail - use dedicated thumbnail if available, otherwise use main image
                    if (!string.IsNullOrWhiteSpace(externalImages.ThumbnailUrl))
                    {
                        firstPicture.ThumbImageUrl = externalImages.ThumbnailUrl;
                        await _logger.InformationAsync($"ProductModelEventConsumer: Product {model.Id} - Using dedicated thumbnail: {externalImages.ThumbnailUrl}");
                    }
                    else
                    {
                        firstPicture.ThumbImageUrl = externalImages.MainImageUrl;
                        await _logger.InformationAsync($"ProductModelEventConsumer: Product {model.Id} - Using main image as thumbnail: {externalImages.MainImageUrl}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ProductModelEventConsumer: Error in ProductOverviewModel handler for product {model.Id}: {ex.Message}", ex);
        }
    }

    private async Task ProcessProductDetailsModel(ProductDetailsModel model)
    {
        try
        {
            var externalImages = await _productExternalImageService.GetProductExternalImagesAsync(model.Id);
            await _logger.InformationAsync($"ProductModelEventConsumer: Retrieved external images for product details {model.Id}: MainImage={externalImages?.MainImageUrl}");

            if (externalImages != null && !string.IsNullOrWhiteSpace(externalImages.MainImageUrl))
            {
                // Replace the default picture model with external image URL
                if (model.DefaultPictureModel != null)
                {
                    await _logger.InformationAsync($"ProductModelEventConsumer: Replacing ProductDetails default image URL from {model.DefaultPictureModel.ImageUrl} to {externalImages.MainImageUrl}");

                    model.DefaultPictureModel.ImageUrl = externalImages.MainImageUrl;
                    model.DefaultPictureModel.FullSizeImageUrl = externalImages.MainImageUrl;
                }

                // Also update the picture models collection
                if (model.PictureModels?.Any() == true)
                {
                    var firstPicture = model.PictureModels.First();
                    await _logger.InformationAsync($"ProductModelEventConsumer: Replacing ProductDetails picture model URL from {firstPicture.ImageUrl} to {externalImages.MainImageUrl}");

                    firstPicture.ImageUrl = externalImages.MainImageUrl;
                    firstPicture.FullSizeImageUrl = externalImages.MainImageUrl;

                    // Update thumbnail - use dedicated thumbnail if available, otherwise use main image
                    if (!string.IsNullOrWhiteSpace(externalImages.ThumbnailUrl))
                    {
                        firstPicture.ThumbImageUrl = externalImages.ThumbnailUrl;
                        await _logger.InformationAsync($"ProductModelEventConsumer: Product {model.Id} - Using dedicated thumbnail: {externalImages.ThumbnailUrl}");
                    }
                    else
                    {
                        firstPicture.ThumbImageUrl = externalImages.MainImageUrl;
                        await _logger.InformationAsync($"ProductModelEventConsumer: Product {model.Id} - Using main image as thumbnail: {externalImages.MainImageUrl}");
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ProductModelEventConsumer: Error in ProductDetailsModel handler for product {model.Id}: {ex.Message}", ex);
        }
    }

    private async Task ProcessCatalogProductsModel(CatalogProductsModel model)
    {
        try
        {
            await _logger.InformationAsync($"ProductModelEventConsumer: Processing CatalogProductsModel with {model.Products?.Count ?? 0} products");

            if (model.Products?.Any() == true)
            {
                foreach (var product in model.Products)
                {
                    await _logger.InformationAsync($"ProductModelEventConsumer: Processing product {product.Id} in CatalogProductsModel");
                    await ProcessProductOverviewModel(product);
                }
            }
            else
            {
                await _logger.InformationAsync($"ProductModelEventConsumer: CatalogProductsModel has no products to process");
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ProductModelEventConsumer: Error in CatalogProductsModel handler: {ex.Message}", ex);
        }
    }

    private async Task ProcessCategoryModel(CategoryModel model)
    {
        try
        {
            await _logger.InformationAsync($"ProductModelEventConsumer: Processing CategoryModel with {model.FeaturedProducts?.Count ?? 0} featured products and {model.CatalogProductsModel?.Products?.Count ?? 0} catalog products");

            // Process featured products
            if (model.FeaturedProducts?.Any() == true)
            {
                foreach (var product in model.FeaturedProducts)
                {
                    await _logger.InformationAsync($"ProductModelEventConsumer: Processing featured product {product.Id} in CategoryModel");
                    await ProcessProductOverviewModel(product);
                }
            }

            // Process catalog products
            if (model.CatalogProductsModel?.Products?.Any() == true)
            {
                foreach (var product in model.CatalogProductsModel.Products)
                {
                    await _logger.InformationAsync($"ProductModelEventConsumer: Processing catalog product {product.Id} in CategoryModel");
                    await ProcessProductOverviewModel(product);
                }
            }

            if ((model.FeaturedProducts?.Count ?? 0) == 0 && (model.CatalogProductsModel?.Products?.Count ?? 0) == 0)
            {
                await _logger.InformationAsync($"ProductModelEventConsumer: CategoryModel has no products to process");
            }
        }
        catch (Exception ex)
        {
            await _logger.ErrorAsync($"ProductModelEventConsumer: Error in CategoryModel handler: {ex.Message}", ex);
        }
    }
}