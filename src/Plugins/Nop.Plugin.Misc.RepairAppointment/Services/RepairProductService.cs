using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public class RepairProductService : IRepairProductService
    {
        private readonly IRepository<RepairProduct> _repairProductRepository;

        public RepairProductService(IRepository<RepairProduct> repairProductRepository)
        {
            _repairProductRepository = repairProductRepository;
        }

        public async Task<RepairProduct?> GetRepairProductByIdAsync(int productId)
        {
            if (productId == 0)
                return null;

            return await _repairProductRepository.GetByIdAsync(productId);
        }

        public async Task<IPagedList<RepairProduct>> GetAllRepairProductsAsync(
            int? categoryId = null,
            string? name = null,
            string? brand = null,
            bool? isActive = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _repairProductRepository.Table;

            if (categoryId.HasValue)
                query = query.Where(p => p.RepairCategoryId == categoryId.Value);

            if (!string.IsNullOrEmpty(name))
                query = query.Where(p => p.Name.Contains(name));

            if (!string.IsNullOrEmpty(brand))
                query = query.Where(p => p.Brand.Contains(brand));

            if (isActive.HasValue)
                query = query.Where(p => p.IsActive == isActive.Value);

            query = query.OrderBy(p => p.DisplayOrder).ThenBy(p => p.Name);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task<IList<RepairProduct>> GetRepairProductsByCategoryIdAsync(int categoryId)
        {
            var query = _repairProductRepository.Table
                .Where(p => p.RepairCategoryId == categoryId && p.IsActive)
                .OrderBy(p => p.DisplayOrder)
                .ThenBy(p => p.Name);

            return await query.ToListAsync();
        }

        public async Task InsertRepairProductAsync(RepairProduct product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.CreatedOnUtc = DateTime.UtcNow;
            await _repairProductRepository.InsertAsync(product);
        }

        public async Task UpdateRepairProductAsync(RepairProduct product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            product.UpdatedOnUtc = DateTime.UtcNow;
            await _repairProductRepository.UpdateAsync(product);
        }

        public async Task DeleteRepairProductAsync(RepairProduct product)
        {
            if (product == null)
                throw new ArgumentNullException(nameof(product));

            await _repairProductRepository.DeleteAsync(product);
        }

        public async Task<IList<RepairProduct>> SearchRepairProductsAsync(int? categoryId, string searchTerm, int maxResults = 10)
        {
            if (string.IsNullOrEmpty(searchTerm))
                return new List<RepairProduct>();

            var query = _repairProductRepository.Table
                .Where(p => p.IsActive &&
                       (p.Name.Contains(searchTerm) ||
                        p.Brand.Contains(searchTerm) ||
                        p.Model.Contains(searchTerm)));

            if (categoryId.HasValue)
                query = query.Where(p => p.RepairCategoryId == categoryId.Value);

            query = query.OrderBy(p => p.DisplayOrder)
                         .ThenBy(p => p.Name)
                         .Take(maxResults);

            return await query.ToListAsync();
        }
    }
}