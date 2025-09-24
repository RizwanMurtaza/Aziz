using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models.RepairCategory
{
    public record RepairCategorySearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Search.Name")]
        public string? SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairCategory.Search.IsActive")]
        public int IsActiveId { get; set; }

        public List<SelectListItem> AvailableActiveOptions { get; set; } = new();
    }
}
