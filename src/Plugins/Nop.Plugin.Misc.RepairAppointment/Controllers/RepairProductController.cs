using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Plugin.Misc.RepairAppointment.Models;
using Nop.Plugin.Misc.RepairAppointment.Services;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.RepairAppointment.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class RepairProductController : BasePluginController
    {
        private readonly IRepairProductService _repairProductService;
        private readonly IRepairCategoryService _repairCategoryService;
        private readonly IRepairTypeService _repairTypeService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        public RepairProductController(
            IRepairProductService repairProductService,
            IRepairCategoryService repairCategoryService,
            IRepairTypeService repairTypeService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _repairProductService = repairProductService;
            _repairCategoryService = repairCategoryService;
            _repairTypeService = repairTypeService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new RepairProductSearchModel();
            model.SetGridPageSize();

            // Load categories for filter
            var categories = await _repairCategoryService.GetAllRepairCategoriesAsync();
            model.AvailableCategories = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
            model.AvailableCategories.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            // Load active options
            model.AvailableActiveOptions = new List<SelectListItem>
            {
                new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" },
                new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Active"), Value = "1" },
                new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Inactive"), Value = "2" }
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairProduct/List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> RepairProductList(RepairProductSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var products = await _repairProductService.GetAllRepairProductsAsync(
                categoryId: searchModel.CategoryId > 0 ? searchModel.CategoryId : null,
                name: searchModel.SearchName,
                brand: searchModel.SearchBrand,
                isActive: searchModel.IsActiveId > 0 ? (searchModel.IsActiveId == 1) : null,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var categories = await _repairCategoryService.GetAllRepairCategoriesAsync();
            var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

            var model = new RepairProductListModel
            {
                Data = products.Select(product => new RepairProductModel
                {
                    Id = product.Id,
                    Name = product.Name,
                    RepairCategoryId = product.RepairCategoryId,
                    CategoryName = categoryDict.ContainsKey(product.RepairCategoryId) ? categoryDict[product.RepairCategoryId] : "None",
                    Brand = product.Brand,
                    ProductModel = product.Model,
                    Description = product.Description,
                    IsActive = product.IsActive,
                    DisplayOrder = product.DisplayOrder
                }).ToList(),
                Draw = searchModel.Draw,
                RecordsTotal = products.TotalCount,
                RecordsFiltered = products.TotalCount
            };

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new RepairProductModel();
            await PrepareRepairProductModelAsync(model);

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairProduct/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RepairProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var product = new RepairProduct
                {
                    Name = model.Name,
                    RepairCategoryId = model.RepairCategoryId,
                    Brand = model.Brand,
                    Model = model.ProductModel,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    CreatedOnUtc = System.DateTime.UtcNow
                };

                await _repairProductService.InsertRepairProductAsync(product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Misc.RepairAppointment.Created"));

                return RedirectToAction("List");
            }

            await PrepareRepairProductModelAsync(model);
            return View("~/Plugins/Misc.RepairAppointment/Views/RepairProduct/Create.cshtml", model);
        }

        [Route("Admin/RepairProduct/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var product = await _repairProductService.GetRepairProductByIdAsync(id);
            if (product == null)
                return RedirectToAction("List");

            var model = new RepairProductModel
            {
                Id = product.Id,
                Name = product.Name,
                RepairCategoryId = product.RepairCategoryId,
                Brand = product.Brand,
                ProductModel = product.Model,
                Description = product.Description,
                IsActive = product.IsActive,
                DisplayOrder = product.DisplayOrder
            };

            await PrepareRepairProductModelAsync(model);

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairProduct/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RepairProductModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var product = await _repairProductService.GetRepairProductByIdAsync(model.Id);
            if (product == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                product.Name = model.Name;
                product.RepairCategoryId = model.RepairCategoryId;
                product.Brand = model.Brand;
                product.Model = model.ProductModel;
                product.Description = model.Description;
                product.IsActive = model.IsActive;
                product.DisplayOrder = model.DisplayOrder;
                product.UpdatedOnUtc = System.DateTime.UtcNow;

                await _repairProductService.UpdateRepairProductAsync(product);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Misc.RepairAppointment.Updated"));

                return RedirectToAction("List");
            }

            await PrepareRepairProductModelAsync(model);
            return View("~/Plugins/Misc.RepairAppointment/Views/RepairProduct/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var product = await _repairProductService.GetRepairProductByIdAsync(id);
            if (product != null)
            {
                await _repairProductService.DeleteRepairProductAsync(product);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Misc.RepairAppointment.Deleted"));
            }

            return Json(new { Result = true });
        }

        [HttpPost]
        public async Task<IActionResult> AddRepairType(int productId, string name, string description, decimal estimatedPrice, int estimatedDurationMinutes, bool isActive, int displayOrder)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var product = await _repairProductService.GetRepairProductByIdAsync(productId);
            if (product == null)
                return Json(new { success = false, message = "Product not found" });

            var repairType = new RepairType
            {
                Name = name,
                RepairCategoryId = product.RepairCategoryId,
                RepairProductId = productId,
                Description = description,
                EstimatedPrice = estimatedPrice,
                EstimatedDurationMinutes = estimatedDurationMinutes,
                IsActive = isActive,
                DisplayOrder = displayOrder,
                CreatedOnUtc = System.DateTime.UtcNow
            };

            await _repairTypeService.InsertRepairTypeAsync(repairType);

            return Json(new {
                success = true,
                id = repairType.Id,
                name = repairType.Name,
                description = repairType.Description,
                estimatedPrice = repairType.EstimatedPrice,
                estimatedDurationMinutes = repairType.EstimatedDurationMinutes,
                isActive = repairType.IsActive,
                displayOrder = repairType.DisplayOrder
            });
        }

        [HttpPost]
        public async Task<IActionResult> UpdateRepairType(int id, string name, string description, decimal estimatedPrice, int estimatedDurationMinutes, bool isActive, int displayOrder)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var repairType = await _repairTypeService.GetRepairTypeByIdAsync(id);
            if (repairType == null)
                return Json(new { success = false, message = "Repair type not found" });

            repairType.Name = name;
            repairType.Description = description;
            repairType.EstimatedPrice = estimatedPrice;
            repairType.EstimatedDurationMinutes = estimatedDurationMinutes;
            repairType.IsActive = isActive;
            repairType.DisplayOrder = displayOrder;
            repairType.UpdatedOnUtc = System.DateTime.UtcNow;

            await _repairTypeService.UpdateRepairTypeAsync(repairType);

            return Json(new { success = true });
        }

        [HttpPost]
        public async Task<IActionResult> DeleteRepairType(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var repairType = await _repairTypeService.GetRepairTypeByIdAsync(id);
            if (repairType != null)
            {
                await _repairTypeService.DeleteRepairTypeAsync(repairType);
            }

            return Json(new { success = true });
        }

        private async Task PrepareRepairProductModelAsync(RepairProductModel model)
        {
            var categories = await _repairCategoryService.GetAllRepairCategoriesAsync();
            model.AvailableCategories = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
                Selected = c.Id == model.RepairCategoryId
            }).ToList();
            model.AvailableCategories.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = "0" });

            // Load repair types for this product (if editing)
            if (model.Id > 0)
            {
                var repairTypes = await _repairTypeService.GetAllRepairTypesAsync(
                    categoryId: model.RepairCategoryId,
                    productId: model.Id);

                model.RepairTypes = repairTypes.Select(rt => new RepairTypeModel
                {
                    Id = rt.Id,
                    Name = rt.Name,
                    RepairCategoryId = rt.RepairCategoryId,
                    RepairProductId = rt.RepairProductId,
                    Description = rt.Description,
                    EstimatedPrice = rt.EstimatedPrice,
                    EstimatedDurationMinutes = rt.EstimatedDurationMinutes,
                    IsActive = rt.IsActive,
                    DisplayOrder = rt.DisplayOrder
                }).ToList();
            }
        }
    }
}