using Nop.Web.Framework.Models;
using Nop.Web.Framework.Mvc.ModelBinding;

namespace Nop.Plugin.Misc.RepairAppointment.Models
{
    public record ConfigurationModel : BaseNopModel
    {
        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.EnableAppointmentSystem")]
        public bool EnableAppointmentSystem { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.SlotDurationMinutes")]
        public int SlotDurationMinutes { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.MaxSlotsPerDay")]
        public int MaxSlotsPerDay { get; set; }

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.BusinessStartTime")]
        public string BusinessStartTime { get; set; } = "09:00";

        [NopResourceDisplayName("Plugins.Misc.RepairAppointment.Configuration.BusinessEndTime")]
        public string BusinessEndTime { get; set; } = "17:00";

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