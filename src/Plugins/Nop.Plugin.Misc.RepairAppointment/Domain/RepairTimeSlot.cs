using Nop.Core;
using System;

namespace Nop.Plugin.Misc.RepairAppointment.Domain
{
    public class RepairTimeSlot : BaseEntity
    {
        public DateTime Date { get; set; }
        public string TimeSlot { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int MaxAppointments { get; set; }
        public int CurrentBookings { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsBlocked { get; set; }
        public string? BlockReason { get; set; }
    }
}