using Nop.Plugin.Misc.RepairAppointment.Models.SlotCapacity;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.RepairAppointment.Models.Requests
{
    /// <summary>
    /// Request model for applying slot capacity changes for a specific date
    /// </summary>
    public record ApplySlotChangesForDateRequest
    {
        public DateTime Date { get; set; }
        public IList<TimeSlotCapacityModel> TimeSlots { get; set; } = new List<TimeSlotCapacityModel>();
    }
}