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
    public class RepairCategoryController : BasePluginController
    {
        private readonly IRepairCategoryService _repairCategoryService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;

        public RepairCategoryController(
            IRepairCategoryService repairCategoryService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService)
        {
            _repairCategoryService = repairCategoryService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new RepairCategorySearchModel();
            model.SetGridPageSize();

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairCategory/List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> CategoryList(RepairCategorySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var categories = await _repairCategoryService.GetAllRepairCategoriesAsync(
                name: searchModel.SearchName,
                isActive: searchModel.IsActiveId == 0 ? null : searchModel.IsActiveId == 1,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = new RepairCategoryListModel
            {
                Data = categories.Select(category => new RepairCategoryModel
                {
                    Id = category.Id,
                    Name = category.Name,
                    Description = category.Description,
                    IsActive = category.IsActive,
                    DisplayOrder = category.DisplayOrder
                }).ToList(),
                Total = categories.TotalCount
            };

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new RepairCategoryModel();
            return View("~/Plugins/Misc.RepairAppointment/Views/RepairCategory/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(RepairCategoryModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (ModelState.IsValid)
            {
                var category = new RepairCategory
                {
                    Name = model.Name,
                    Description = model.Description,
                    IsActive = model.IsActive,
                    DisplayOrder = model.DisplayOrder
                };

                await _repairCategoryService.InsertRepairCategoryAsync(category);
                _notificationService.SuccessNotification("Category created successfully");

                return RedirectToAction("List");
            }

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairCategory/Create.cshtml", model);
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var category = await _repairCategoryService.GetRepairCategoryByIdAsync(id);
            if (category == null)
                return RedirectToAction("List");

            var model = new RepairCategoryModel
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive,
                DisplayOrder = category.DisplayOrder
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairCategory/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RepairCategoryModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var category = await _repairCategoryService.GetRepairCategoryByIdAsync(model.Id);
            if (category == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                category.Name = model.Name;
                category.Description = model.Description;
                category.IsActive = model.IsActive;
                category.DisplayOrder = model.DisplayOrder;

                await _repairCategoryService.UpdateRepairCategoryAsync(category);
                _notificationService.SuccessNotification("Category updated successfully");

                return RedirectToAction("List");
            }

            return View("~/Plugins/Misc.RepairAppointment/Views/RepairCategory/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var category = await _repairCategoryService.GetRepairCategoryByIdAsync(id);
            if (category == null)
                return Json(new { Result = false });

            await _repairCategoryService.DeleteRepairCategoryAsync(category);

            return Json(new { Result = true });
        }
    }
}