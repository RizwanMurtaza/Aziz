using System;

namespace Nop.Plugin.Misc.RepairAppointment.Models.Requests
{
    /// <summary>
    /// Request model for getting time slots for a specific date
    /// </summary>
    public record GetTimeSlotsForDateRequest
    {
        public DateTime Date { get; set; }
    }
}