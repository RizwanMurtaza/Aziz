using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models
{
    public record RepairProductModel : BaseNopEntityModel
    {
        public RepairProductModel()
        {
            AvailableCategories = new List<SelectListItem>();
            RepairTypes = new List<RepairTypeModel>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Fields.RepairCategoryId")]
        [Required]
        public int RepairCategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Fields.Name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Fields.Brand")]
        public string Brand { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Fields.Model")]
        public string ProductModel { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Fields.Description")]
        public string Description { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Fields.IsActive")]
        public bool IsActive { get; set; } = true;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<RepairTypeModel> RepairTypes { get; set; }
    }

    public record RepairProductListModel : BasePagedListModel<RepairProductModel>
    {
    }

    public record RepairProductSearchModel : BaseSearchModel
    {
        public RepairProductSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableActiveOptions = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Search.CategoryId")]
        public int CategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Search.Name")]
        public string? SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Search.Brand")]
        public string? SearchBrand { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairProduct.Search.IsActive")]
        public int IsActiveId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableActiveOptions { get; set; }
    }
}