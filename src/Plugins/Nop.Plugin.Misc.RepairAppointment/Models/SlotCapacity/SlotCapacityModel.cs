using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System;
using System.ComponentModel.DataAnnotations;

namespace Nop.Plugin.Misc.RepairAppointment.Models.SlotCapacity
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
}