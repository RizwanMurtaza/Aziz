using Nop.Core;
using System;

namespace Nop.Plugin.Misc.RepairAppointment.Domain
{
    /// <summary>
    /// Represents slot capacity override for specific date and time combinations
    /// </summary>
    public class SlotCapacity : BaseEntity
    {
        /// <summary>
        /// The specific date for this capacity override
        /// </summary>
        public DateTime Date { get; set; }

        /// <summary>
        /// Start time for this slot (e.g., "09:00")
        /// </summary>
        public TimeSpan StartTime { get; set; }

        /// <summary>
        /// End time for this slot (e.g., "09:30")
        /// </summary>
        public TimeSpan EndTime { get; set; }

        /// <summary>
        /// Maximum number of appointments allowed for this specific slot
        /// Overrides the default capacity from settings
        /// </summary>
        public int MaxAppointments { get; set; }

        /// <summary>
        /// Current number of booked appointments for this slot
        /// </summary>
        public int CurrentBookings { get; set; } = 0;

        /// <summary>
        /// Whether this slot is available for booking
        /// </summary>
        public bool IsActive { get; set; } = true;

        /// <summary>
        /// Optional notes for this slot (e.g., "Extra technician available", "Limited capacity due to maintenance")
        /// </summary>
        public string? Notes { get; set; }

        /// <summary>
        /// When this capacity override was created
        /// </summary>
        public DateTime CreatedOnUtc { get; set; }

        /// <summary>
        /// When this capacity override was last modified
        /// </summary>
        public DateTime? ModifiedOnUtc { get; set; }
    }
}