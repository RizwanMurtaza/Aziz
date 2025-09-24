using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models.RepairType
{
    public record RepairTypeSearchModel : BaseSearchModel
    {
        public RepairTypeSearchModel()
        {
            AvailableCategories = new List<SelectListItem>();
            AvailableProducts = new List<SelectListItem>();
            AvailableActiveOptions = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Search.CategoryId")]
        public int CategoryId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Search.ProductId")]
        public int ProductId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Search.Name")]
        public string? SearchName { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.RepairType.Search.IsActive")]
        public int IsActiveId { get; set; }

        public IList<SelectListItem> AvailableCategories { get; set; }
        public IList<SelectListItem> AvailableProducts { get; set; }
        public IList<SelectListItem> AvailableActiveOptions { get; set; }
    }
}
