using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public class RepairTypeService : IRepairTypeService
    {
        private readonly IRepository<RepairType> _repairTypeRepository;

        public RepairTypeService(IRepository<RepairType> repairTypeRepository)
        {
            _repairTypeRepository = repairTypeRepository;
        }

        public async Task<RepairType?> GetRepairTypeByIdAsync(int typeId)
        {
            if (typeId == 0)
                return null;

            return await _repairTypeRepository.GetByIdAsync(typeId);
        }

        public async Task<IPagedList<RepairType>> GetAllRepairTypesAsync(
            int? categoryId = null,
            int? productId = null,
            string? name = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _repairTypeRepository.Table;

            if (categoryId.HasValue)
                query = query.Where(t => t.RepairCategoryId == categoryId.Value);

            if (productId.HasValue)
                query = query.Where(t => t.RepairProductId == productId.Value);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(t => t.Name.Contains(name));

            if (isActive.HasValue)
                query = query.Where(t => t.IsActive == isActive.Value);

            query = query.OrderBy(t => t.DisplayOrder).ThenBy(t => t.Name);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<RepairType>> GetRepairTypesByCategoryIdAsync(int categoryId)
        {
            var query = _repairTypeRepository.Table
                .Where(t => t.RepairCategoryId == categoryId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Name);

            return await query.ToListAsync();
        }

        public async Task<IList<RepairType>> GetRepairTypesByProductIdAsync(int productId)
        {
            var query = _repairTypeRepository.Table
                .Where(t => t.RepairProductId == productId && t.IsActive)
                .OrderBy(t => t.DisplayOrder)
                .ThenBy(t => t.Name);

            return await query.ToListAsync();
        }

        public async Task InsertRepairTypeAsync(RepairType repairType)
        {
            if (repairType == null)
                throw new ArgumentNullException(nameof(repairType));

            repairType.CreatedOnUtc = DateTime.UtcNow;
            await _repairTypeRepository.InsertAsync(repairType);
        }

        public async Task UpdateRepairTypeAsync(RepairType repairType)
        {
            if (repairType == null)
                throw new ArgumentNullException(nameof(repairType));

            repairType.UpdatedOnUtc = DateTime.UtcNow;
            await _repairTypeRepository.UpdateAsync(repairType);
        }

        public async Task DeleteRepairTypeAsync(RepairType repairType)
        {
            if (repairType == null)
                throw new ArgumentNullException(nameof(repairType));

            await _repairTypeRepository.DeleteAsync(repairType);
        }

        public async Task<IList<RepairType>> SearchRepairTypesAsync(int? categoryId, int? productId, string searchTerm, int maxResults = 10)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<RepairType>();

            var query = _repairTypeRepository.Table
                .Where(t => t.IsActive && t.Name.Contains(searchTerm));

            if (categoryId.HasValue)
                query = query.Where(t => t.RepairCategoryId == categoryId.Value);

            if (productId.HasValue)
                query = query.Where(t => t.RepairProductId == productId.Value);

            query = query.OrderBy(t => t.DisplayOrder)
                         .ThenBy(t => t.Name)
                         .Take(maxResults);

            return await query.ToListAsync();
        }
    }
}