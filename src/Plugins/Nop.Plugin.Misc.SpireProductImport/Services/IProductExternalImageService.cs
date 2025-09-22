using Nop.Plugin.Misc.SpireProductImport.Models;

namespace Nop.Plugin.Misc.SpireProductImport.Services;

public interface IProductExternalImageService
{
    Task SaveProductExternalImagesAsync(int productId, string mainImageUrl, string thumbnailUrl);
    Task<ProductExternalImageModel> GetProductExternalImagesAsync(int productId);
    Task DeleteProductExternalImagesAsync(int productId);
}