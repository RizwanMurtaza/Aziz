using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models.RepairType
{
    public record RepairTypeModel : BaseNopEntityModel
    {
        public RepairTypeModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableProducts = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.RepairCategoryId")]
        [Required]
        public int RepairCategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.RepairProductId")]
        public int? RepairProductId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.Name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.Description")]
        public string Description { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.EstimatedPrice")]
        [Required]
        public decimal EstimatedPrice { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.EstimatedDurationMinutes")]
        [Required]
        public int EstimatedDurationMinutes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.IsActive")]
        public bool IsActive { get; set; } = true;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }

        public string CategoryName { get; set; } = string.Empty;
        public string? ProductName { get; set; }
        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableProducts { get; set; }
    }
}
