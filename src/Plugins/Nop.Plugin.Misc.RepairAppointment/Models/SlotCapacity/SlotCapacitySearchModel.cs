using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;

namespace Nop.Plugin.Misc.RepairAppointment.Models.SlotCapacity
{
    /// <summary>
    /// Represents a slot capacity search model
    /// </summary>
    public record SlotCapacitySearchModel : BaseSearchModel
    {
        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Search.Date")]
        public DateTime? SearchDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Search.FromDate")]
        public DateTime? FromDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Search.ToDate")]
        public DateTime? ToDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Search.IsActive")]
        public bool? IsActive { get; set; }
    }
}