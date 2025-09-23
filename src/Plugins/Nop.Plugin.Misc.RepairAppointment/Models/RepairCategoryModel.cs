using System.ComponentModel.DataAnnotations;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models
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

    public record RepairCategoryListModel : BasePagedListModel<RepairCategoryModel>
    {
        public int Total { get; set; }
    }

    public record RepairCategorySearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Search.Name")]
        public string? SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Search.IsActive")]
        public int IsActiveId { get; set; }
    }
}