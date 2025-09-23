using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public interface IRepairProductService
    {
        Task<RepairProduct?> GetRepairProductByIdAsync(int productId);
        Task<IPagedList<RepairProduct>> GetAllRepairProductsAsync(
            int? categoryId = null,
            string? name = null,
            string? brand = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
        Task<IList<RepairProduct>> GetRepairProductsByCategoryIdAsync(int categoryId);
        Task InsertRepairProductAsync(RepairProduct product);
        Task UpdateRepairProductAsync(RepairProduct product);
        Task DeleteRepairProductAsync(RepairProduct product);
        Task<IList<RepairProduct>> SearchRepairProductsAsync(int? categoryId, string searchTerm, int maxResults = 10);
    }
}