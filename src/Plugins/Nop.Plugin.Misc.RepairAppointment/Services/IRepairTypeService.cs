using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public interface IRepairTypeService
    {
        Task<RepairType?> GetRepairTypeByIdAsync(int typeId);
        Task<IPagedList<RepairType>> GetAllRepairTypesAsync(
            int? categoryId = null,
            int? productId = null,
            string? name = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
        Task<IList<RepairType>> GetRepairTypesByCategoryIdAsync(int categoryId);
        Task<IList<RepairType>> GetRepairTypesByProductIdAsync(int productId);
        Task InsertRepairTypeAsync(RepairType repairType);
        Task UpdateRepairTypeAsync(RepairType repairType);
        Task DeleteRepairTypeAsync(RepairType repairType);
        Task<IList<RepairType>> SearchRepairTypesAsync(int? categoryId, int? productId, string searchTerm, int maxResults = 10);
    }
}