using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Plugin.Misc.RepairAppointment.Models.RepairType;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models.RepairProduct
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
}
