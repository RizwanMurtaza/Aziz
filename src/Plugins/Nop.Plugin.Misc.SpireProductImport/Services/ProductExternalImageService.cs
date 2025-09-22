using Nop.Core.Domain.Common;
using Nop.Data;
using Nop.Plugin.Misc.SpireProductImport.Models;
using Nop.Services.Common;
using System.Text.Json;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

public class ProductExternalImageService : IProductExternalImageService
{
    private readonly IGenericAttributeService _genericAttributeService;
    private readonly IRepository<GenericAttribute> _genericAttributeRepository;
    private const string EXTERNAL_IMAGE_KEY = "SpireProductImport.ExternalImages";

    public ProductExternalImageService(
        IGenericAttributeService genericAttributeService,
        IRepository<GenericAttribute> genericAttributeRepository)
    {
        _genericAttributeService = genericAttributeService;
        _genericAttributeRepository = genericAttributeRepository;
    }

    public async Task SaveProductExternalImagesAsync(int productId, string mainImageUrl, string thumbnailUrl)
    {
        var imageData = new ProductExternalImageModel
        {
            ProductId = productId,
            MainImageUrl = mainImageUrl,
            ThumbnailUrl = thumbnailUrl
        };

        var jsonData = JsonSerializer.Serialize(imageData);

        // Save as a generic attribute for the product
        // We'll use a custom entity approach since Product isn't a BaseEntity in this context
        var existingAttribute = (await _genericAttributeRepository.GetAllAsync(query =>
            query.Where(ga => ga.EntityId == productId &&
                             ga.KeyGroup == "Product" &&
                             ga.Key == EXTERNAL_IMAGE_KEY))).FirstOrDefault();

        if (existingAttribute != null)
        {
            existingAttribute.Value = jsonData;
            await _genericAttributeRepository.UpdateAsync(existingAttribute);
        }
        else
        {
            var newAttribute = new GenericAttribute
            {
                EntityId = productId,
                KeyGroup = "Product",
                Key = EXTERNAL_IMAGE_KEY,
                Value = jsonData,
                StoreId = 0
            };
            await _genericAttributeRepository.InsertAsync(newAttribute);
        }
    }

    public async Task<ProductExternalImageModel> GetProductExternalImagesAsync(int productId)
    {
        var attribute = (await _genericAttributeRepository.GetAllAsync(query =>
            query.Where(ga => ga.EntityId == productId &&
                             ga.KeyGroup == "Product" &&
                             ga.Key == EXTERNAL_IMAGE_KEY &&
                             ga.StoreId == 0))).FirstOrDefault();

        var attributeValue = attribute?.Value;

        if (string.IsNullOrEmpty(attributeValue))
            return null;

        try
        {
            return JsonSerializer.Deserialize<ProductExternalImageModel>(attributeValue);
        }
        catch
        {
            return null;
        }
    }

    public async Task DeleteProductExternalImagesAsync(int productId)
    {
        var attributes = await _genericAttributeRepository.GetAllAsync(query =>
            query.Where(ga => ga.EntityId == productId &&
                             ga.KeyGroup == "Product" &&
                             ga.Key == EXTERNAL_IMAGE_KEY));

        if (attributes?.Any() == true)
        {
            await _genericAttributeRepository.DeleteAsync(attributes);
        }
    }
}