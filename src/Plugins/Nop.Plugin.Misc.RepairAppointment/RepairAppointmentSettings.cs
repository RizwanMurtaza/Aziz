using Nop.Core.Configuration;

namespace Nop.Plugin.Misc.RepairAppointment
{
    public class RepairAppointmentSettings : ISettings
    {
        public bool EnableAppointmentSystem { get; set; }
        public int SlotDurationMinutes { get; set; } = 30;
        public int MaxSlotsPerDay { get; set; } = 16;
        public int MaxAppointmentsPerSlot { get; set; } = 1;
        public string BusinessStartTime { get; set; } = "09:00";
        public string BusinessEndTime { get; set; } = "17:00";
        public string WorkingDays { get; set; } = "1,2,3,4,5"; // Monday to Friday by default
        public bool SendConfirmationEmail { get; set; } = true;
        public bool SendReminderEmail { get; set; } = true;
        public int ReminderHoursBeforeAppointment { get; set; } = 24;
        public string? BlockedDates { get; set; }
        public int MaxAdvanceBookingDays { get; set; } = 30;
        public bool RequireCustomerLogin { get; set; }
    }
}