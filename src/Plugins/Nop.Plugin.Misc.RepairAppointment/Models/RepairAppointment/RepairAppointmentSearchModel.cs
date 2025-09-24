using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models.RepairAppointment
{
    public record RepairAppointmentSearchModel : BaseSearchModel
    {
        public RepairAppointmentSearchModel()
        {
            AvailableStatuses = new List<SelectListItem>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.FromDate")]
        public DateTime? FromDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.ToDate")]
        public DateTime? ToDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.Status")]
        public int StatusId { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Search.SearchText")]
        public string? SearchText { get; set; }

        public IList<SelectListItem> AvailableStatuses { get; set; }
    }
}
