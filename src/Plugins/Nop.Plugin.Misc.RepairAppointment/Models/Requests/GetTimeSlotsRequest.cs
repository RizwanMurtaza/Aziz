using System;

namespace Nop.Plugin.Misc.RepairAppointment.Models.Requests
{
    /// <summary>
    /// Request model for getting time slots for weekdays
    /// </summary>
    public record GetTimeSlotsRequest
    {
        public int[] Weekdays { get; set; } = Array.Empty<int>();
    }
}