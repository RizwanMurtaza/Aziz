using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.RepairAppointment.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        public ConfigurationModel()
        {
            SelectedWorkingDays = new List<int>();
        }
        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.EnableAppointmentSystem")]
        public bool EnableAppointmentSystem { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.SlotDurationMinutes")]
        public int SlotDurationMinutes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.MaxSlotsPerDay")]
        public int MaxSlotsPerDay { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.MaxAppointmentsPerSlot")]
        public int MaxAppointmentsPerSlot { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.BusinessStartTime")]
        public string BusinessStartTime { get; set; } = "09:00";

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.BusinessEndTime")]
        public string BusinessEndTime { get; set; } = "17:00";

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.WorkingDays")]
        public IList<int> SelectedWorkingDays { get; set; }

        public IList<SelectListItem> AvailableWorkingDays { get; set; } = new List<SelectListItem>
        {
            new() { Text = "Monday", Value = "1" },
            new() { Text = "Tuesday", Value = "2" },
            new() { Text = "Wednesday", Value = "3" },
            new() { Text = "Thursday", Value = "4" },
            new() { Text = "Friday", Value = "5" },
            new() { Text = "Saturday", Value = "6" },
            new() { Text = "Sunday", Value = "0" }
        };

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.SendConfirmationEmail")]
        public bool SendConfirmationEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.SendReminderEmail")]
        public bool SendReminderEmail { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.ReminderHoursBeforeAppointment")]
        public int ReminderHoursBeforeAppointment { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.MaxAdvanceBookingDays")]
        public int MaxAdvanceBookingDays { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.RequireCustomerLogin")]
        public bool RequireCustomerLogin { get; set; }
    }
}