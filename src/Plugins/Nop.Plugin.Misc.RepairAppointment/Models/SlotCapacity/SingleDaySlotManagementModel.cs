using Nop.Web.Framework.Models;
using System;
using System.Collections.Generic;

namespace Nop.Plugin.Misc.RepairAppointment.Models.SlotCapacity
{
    /// <summary>
    /// Represents a single day slot management model for Add New Slot functionality
    /// </summary>
    public record SingleDaySlotManagementModel : BaseNopModel
    {
        public DateTime SelectedDate { get; set; }
        public IList<int> WorkingDays { get; set; } = new List<int>();
    }
}