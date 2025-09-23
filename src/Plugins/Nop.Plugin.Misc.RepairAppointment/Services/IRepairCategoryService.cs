using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public interface IRepairCategoryService
    {
        Task<RepairCategory?> GetRepairCategoryByIdAsync(int categoryId);
        Task<IPagedList<RepairCategory>> GetAllRepairCategoriesAsync(
            string? name = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
        Task<IList<RepairCategory>> GetActiveRepairCategoriesAsync();
        Task InsertRepairCategoryAsync(RepairCategory category);
        Task UpdateRepairCategoryAsync(RepairCategory category);
        Task DeleteRepairCategoryAsync(RepairCategory category);
        Task<IList<RepairCategory>> SearchRepairCategoriesAsync(string searchTerm, int maxResults = 10);
    }
}