using System;

namespace Nop.Plugin.Misc.RepairAppointment.Models.SlotCapacity
{
    /// <summary>
    /// Represents a time slot capacity model for bulk management operations
    /// </summary>
    public record TimeSlotCapacityModel
    {
        public string TimeSlot { get; set; } = string.Empty;
        public TimeSpan StartTime { get; set; }
        public TimeSpan EndTime { get; set; }
        public int DefaultCapacity { get; set; }
        public int NewCapacity { get; set; }
        public string StartTimeFormatted { get; set; } = string.Empty;
        public string EndTimeFormatted { get; set; } = string.Empty;
        public bool Exists { get; set; } = false;
    }
}