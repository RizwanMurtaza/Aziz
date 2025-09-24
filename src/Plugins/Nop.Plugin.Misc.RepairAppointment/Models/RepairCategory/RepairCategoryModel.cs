using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models.RepairCategory
{
    public record RepairCategoryModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Fields.Name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Fields.Description")]
        public string Description { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Fields.IsActive")]
        public bool IsActive { get; set; } = true;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Fields.DisplayOrder")]
        public int DisplayOrder { get; set; }
    }
}
