using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models.RepairProduct
{
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
