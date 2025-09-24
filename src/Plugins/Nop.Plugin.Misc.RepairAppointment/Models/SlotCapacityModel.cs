using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.RepairAppointment.Models
{
    /// <summary>
    /// Represents a slot capacity model
    /// </summary>
    public record SlotCapacityModel : BaseNopEntityModel
    {
        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.Date")]
        [Required]
        public DateTime Date { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.StartTime")]
        [Required]
        public string StartTime { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.EndTime")]
        [Required]
        public string EndTime { get; set; } = string.Empty;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.MaxAppointments")]
        [Required]
        [Range(0, 100, ErrorMessage = "Max appointments must be between 0 and 100")]
        public int MaxAppointments { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.CurrentBookings")]
        public int CurrentBookings { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.AvailableSlots")]
        public int AvailableSlots => MaxAppointments - CurrentBookings;

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.IsActive")]
        public bool IsActive { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
        public string? Notes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.CreatedOn")]
        public DateTime CreatedOn { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.SlotCapacity.Fields.ModifiedOn")]
        public DateTime? ModifiedOn { get; set; }

        // Display properties
        public string TimeSlot => $"{StartTime} - {EndTime}";
        public string FormattedDate => Date.ToString("dddd, MMMM dd, yyyy");
        public string StatusText => IsActive ? "Active" : "Inactive";
        public string CapacityStatus => $"{CurrentBookings}/{MaxAppointments}";
    }

    /// <summary>
    /// Represents a slot capacity list model
    /// </summary>
    public record SlotCapacityListModel : BasePagedListModel<SlotCapacityModel>
    {
    }

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

    public record TimeSlotCapacityModel
    {
        public string TimeSlot { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DefaultCapacity { get; set; }
        public int NewCapacity { get; set; }
        public string StartTimeFormatted { get; set; } = string.Empty;
        public string EndTimeFormatted { get; set; } = string.Empty;
    }

    public record GetTimeSlotsRequest
    {
        public int[] Weekdays { get; set; } = Array.Empty<int>();
    }

    public record ApplyBulkChangesRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int[] SelectedWeekdays { get; set; } = Array.Empty<int>();
        public TimeSlotCapacityModel[] TimeSlots { get; set; } = Array.Empty<TimeSlotCapacityModel>();
    }
}