using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.RepairAppointment.Models.SlotCapacity
{
    /// <summary>
    /// Represents a bulk slot management model
    /// </summary>
    public record BulkSlotManagementModel : BaseNopModel
    {
        public BulkSlotManagementModel()
        {
            SelectedWeekdays = new List<int>();
            TimeSlots = new List<TimeSlotCapacityModel>();
        }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.BulkSlot.FromDate")]
        [Required]
        public DateTime FromDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.BulkSlot.ToDate")]
        [Required]
        public DateTime ToDate { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.BulkSlot.SelectedWeekdays")]
        public IList<int> SelectedWeekdays { get; set; }

        public IList<SelectListItem> AvailableWeekdays { get; set; } = new List<SelectListItem>
        {
            new() { Text = "Monday", Value = "1" },
            new() { Text = "Tuesday", Value = "2" },
            new() { Text = "Wednesday", Value = "3" },
            new() { Text = "Thursday", Value = "4" },
            new() { Text = "Friday", Value = "5" },
            new() { Text = "Saturday", Value = "6" },
            new() { Text = "Sunday", Value = "0" }
        };

        public IList<TimeSlotCapacityModel> TimeSlots { get; set; }
    }
}