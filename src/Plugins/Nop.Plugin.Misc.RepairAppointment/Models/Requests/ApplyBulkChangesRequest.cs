using Nop.Plugin.Misc.RepairAppointment.Models.SlotCapacity;
using System;

namespace Nop.Plugin.Misc.RepairAppointment.Models.Requests
{
    /// <summary>
    /// Request model for applying bulk slot capacity changes
    /// </summary>
    public record ApplyBulkChangesRequest
    {
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public int[] SelectedWeekdays { get; set; } = Array.Empty<int>();
        public TimeSlotCapacityModel[] TimeSlots { get; set; } = Array.Empty<TimeSlotCapacityModel>();
    }
}