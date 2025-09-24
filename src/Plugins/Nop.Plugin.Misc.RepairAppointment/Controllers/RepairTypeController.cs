using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Plugin.Misc.RepairAppointment.Models.RepairType;
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
    public class RepairTypeController : BasePluginController
    {
        private readonly IRepairTypeService _repairTypeService;
        private readonly IRepairCategoryService _repairCategoryService;
        private readonly IRepairProductService _repairProductService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        public RepairTypeController(
            IRepairTypeService repairTypeService,
            IRepairCategoryService repairCategoryService,
            IRepairProductService repairProductService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _repairTypeService = repairTypeService;
            _repairCategoryService = repairCategoryService;
            _repairProductService = repairProductService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new RepairTypeSearchModel();
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

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairType/List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> RepairTypeList(RepairTypeSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var repairTypes = await _repairTypeService.GetAllRepairTypesAsync(
                categoryId: searchModel.CategoryId > 0 ? searchModel.CategoryId : null,
                productId: searchModel.ProductId > 0 ? searchModel.ProductId : null,
                name: searchModel.SearchName,
                isActive: searchModel.IsActiveId > 0 ? (searchModel.IsActiveId == 1) : null,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var categories = await _repairCategoryService.GetAllRepairCategoriesAsync();
            var categoryDict = categories.ToDictionary(c => c.Id, c => c.Name);

            var products = await _repairProductService.GetAllRepairProductsAsync();
            var productDict = products.ToDictionary(p => p.Id, p => p.Name);

            var model = new RepairTypeListModel
            {
                Data = repairTypes.Select(repairType => new RepairTypeModel
                {
                    Id = repairType.Id,
                    Name = repairType.Name,
                    RepairCategoryId = repairType.RepairCategoryId,
                    CategoryName = categoryDict.ContainsKey(repairType.RepairCategoryId) ? categoryDict[repairType.RepairCategoryId] : "None",
                    RepairProductId = repairType.RepairProductId,
                    ProductName = repairType.RepairProductId.HasValue && productDict.ContainsKey(repairType.RepairProductId.Value)
                        ? productDict[repairType.RepairProductId.Value] : "None",
                    EstimatedPrice = repairType.EstimatedPrice,
                    EstimatedDurationMinutes = repairType.EstimatedDurationMinutes,
                    IsActive = repairType.IsActive,
                    DisplayOrder = repairType.DisplayOrder
                }).ToList(),
                Draw = searchModel.Draw,
                RecordsTotal = repairTypes.TotalCount,
                RecordsFiltered = repairTypes.TotalCount
            };

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new RepairTypeModel();
            await PrepareRepairTypeModelAsync(model);

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairType/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RepairTypeModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var repairType = new RepairType
                {
                    Name = model.Name,
                    RepairCategoryId = model.RepairCategoryId,
                    RepairProductId = model.RepairProductId > 0 ? model.RepairProductId : null,
                    Description = model.Description,
                    EstimatedPrice = model.EstimatedPrice,
                    EstimatedDurationMinutes = model.EstimatedDurationMinutes,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder,
                    CreatedOnUtc = System.DateTime.UtcNow
                };

                await _repairTypeService.InsertRepairTypeAsync(repairType);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Misc.RepairAppointment.Created"));

                return RedirectToAction("List");
            }

            await PrepareRepairTypeModelAsync(model);
            return View("~/Plugins/Misc.RepairAppointment/Views/RepairType/Create.cshtml", model);
        }

        [Route("Admin/RepairType/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var repairType = await _repairTypeService.GetRepairTypeByIdAsync(id);
            if (repairType == null)
                return RedirectToAction("List");

            var model = new RepairTypeModel
            {
                Id = repairType.Id,
                Name = repairType.Name,
                RepairCategoryId = repairType.RepairCategoryId,
                RepairProductId = repairType.RepairProductId ?? 0,
                Description = repairType.Description,
                EstimatedPrice = repairType.EstimatedPrice,
                EstimatedDurationMinutes = repairType.EstimatedDurationMinutes,
                IsActive = repairType.IsActive,
                DisplayOrder = repairType.DisplayOrder
            };

            await PrepareRepairTypeModelAsync(model);

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairType/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RepairTypeModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var repairType = await _repairTypeService.GetRepairTypeByIdAsync(model.Id);
            if (repairType == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                repairType.Name = model.Name;
                repairType.RepairCategoryId = model.RepairCategoryId;
                repairType.RepairProductId = model.RepairProductId > 0 ? model.RepairProductId : null;
                repairType.Description = model.Description;
                repairType.EstimatedPrice = model.EstimatedPrice;
                repairType.EstimatedDurationMinutes = model.EstimatedDurationMinutes;
                repairType.IsActive = model.IsActive;
                repairType.DisplayOrder = model.DisplayOrder;
                repairType.UpdatedOnUtc = System.DateTime.UtcNow;

                await _repairTypeService.UpdateRepairTypeAsync(repairType);

                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Misc.RepairAppointment.Updated"));

                return RedirectToAction("List");
            }

            await PrepareRepairTypeModelAsync(model);
            return View("~/Plugins/Misc.RepairAppointment/Views/RepairType/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var repairType = await _repairTypeService.GetRepairTypeByIdAsync(id);
            if (repairType != null)
            {
                await _repairTypeService.DeleteRepairTypeAsync(repairType);
                _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Misc.RepairAppointment.Deleted"));
            }

            return Json(new { Result = true });
        }

        [HttpGet]
        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _repairProductService.GetAllRepairProductsAsync(categoryId: categoryId > 0 ? categoryId : null);
            var productList = products.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString()
            }).ToList();

            return Json(productList);
        }

        private async Task PrepareRepairTypeModelAsync(RepairTypeModel model)
        {
            var categories = await _repairCategoryService.GetAllRepairCategoriesAsync();
            model.AvailableCategories = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString(),
                Selected = c.Id == model.RepairCategoryId
            }).ToList();
            model.AvailableCategories.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.Select"), Value = "0" });

            var products = await _repairProductService.GetAllRepairProductsAsync(categoryId: model.RepairCategoryId > 0 ? model.RepairCategoryId : null);
            model.AvailableProducts = products.Select(p => new SelectListItem
            {
                Text = p.Name,
                Value = p.Id.ToString(),
                Selected = p.Id == model.RepairProductId
            }).ToList();
            model.AvailableProducts.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.None"), Value = "0" });
        }
    }
}