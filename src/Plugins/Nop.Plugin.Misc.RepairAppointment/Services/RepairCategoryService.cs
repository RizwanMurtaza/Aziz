using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public class RepairCategoryService : IRepairCategoryService
    {
        private readonly IRepository<RepairCategory> _repairCategoryRepository;

        public RepairCategoryService(IRepository<RepairCategory> repairCategoryRepository)
        {
            _repairCategoryRepository = repairCategoryRepository;
        }

        public async Task<RepairCategory?> GetRepairCategoryByIdAsync(int categoryId)
        {
            if (categoryId == 0)
                return null;

            return await _repairCategoryRepository.GetByIdAsync(categoryId);
        }

        public async Task<IPagedList<RepairCategory>> GetAllRepairCategoriesAsync(
            string? name = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _repairCategoryRepository.Table;

            if (!string.IsNullOrEmpty(name))
                query = query.Where(c => c.Name.Contains(name));

            if (isActive.HasValue)
                query = query.Where(c => c.IsActive == isActive.Value);

            query = query.OrderBy(c => c.DisplayOrder).ThenBy(c => c.Name);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<RepairCategory>> GetActiveRepairCategoriesAsync()
        {
            var query = _repairCategoryRepository.Table
                .Where(c => c.IsActive)
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name);

            return await query.ToListAsync();
        }

        public async Task InsertRepairCategoryAsync(RepairCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.CreatedOnUtc = DateTime.UtcNow;
            await _repairCategoryRepository.InsertAsync(category);
        }

        public async Task UpdateRepairCategoryAsync(RepairCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            category.UpdatedOnUtc = DateTime.UtcNow;
            await _repairCategoryRepository.UpdateAsync(category);
        }

        public async Task DeleteRepairCategoryAsync(RepairCategory category)
        {
            if (category == null)
                throw new ArgumentNullException(nameof(category));

            await _repairCategoryRepository.DeleteAsync(category);
        }

        public async Task<IList<RepairCategory>> SearchRepairCategoriesAsync(string searchTerm, int maxResults = 10)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<RepairCategory>();

            var query = _repairCategoryRepository.Table
                .Where(c => c.IsActive && c.Name.Contains(searchTerm))
                .OrderBy(c => c.DisplayOrder)
                .ThenBy(c => c.Name)
                .Take(maxResults);

            return await query.ToListAsync();
        }
    }
}