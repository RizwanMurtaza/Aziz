using Nop.Core;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    /// <summary>
    /// Service for managing slot capacity overrides
    /// </summary>
    public interface ISlotCapacityService
    {
        /// <summary>
        /// Gets a slot capacity by identifier
        /// </summary>
        /// <param name="id">Slot capacity identifier</param>
        /// <returns>Slot capacity</returns>
        Task<SlotCapacity?> GetSlotCapacityByIdAsync(int id);

        /// <summary>
        /// Gets slot capacity for a specific date and time
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>Slot capacity or null if no override exists</returns>
        Task<SlotCapacity?> GetSlotCapacityAsync(DateTime date, TimeSpan startTime, TimeSpan endTime);

        /// <summary>
        /// Gets all slot capacity overrides for a specific date
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="activeOnly">Whether to return only active slots</param>
        /// <returns>List of slot capacities</returns>
        Task<IList<SlotCapacity>> GetSlotCapacitiesByDateAsync(DateTime date, bool activeOnly = true);

        /// <summary>
        /// Gets all slot capacity overrides within a date range
        /// </summary>
        /// <param name="fromDate">From date</param>
        /// <param name="toDate">To date</param>
        /// <param name="activeOnly">Whether to return only active slots</param>
        /// <param name="pageIndex">Page index</param>
        /// <param name="pageSize">Page size</param>
        /// <returns>Paged list of slot capacities</returns>
        Task<IPagedList<SlotCapacity>> GetSlotCapacitiesAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? activeOnly = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);

        /// <summary>
        /// Inserts a slot capacity
        /// </summary>
        /// <param name="slotCapacity">Slot capacity</param>
        Task InsertSlotCapacityAsync(SlotCapacity slotCapacity);

        /// <summary>
        /// Updates a slot capacity
        /// </summary>
        /// <param name="slotCapacity">Slot capacity</param>
        Task UpdateSlotCapacityAsync(SlotCapacity slotCapacity);

        /// <summary>
        /// Deletes a slot capacity
        /// </summary>
        /// <param name="slotCapacity">Slot capacity</param>
        Task DeleteSlotCapacityAsync(SlotCapacity slotCapacity);

        /// <summary>
        /// Deletes all slot capacity data
        /// </summary>
        Task DeleteAllSlotCapacitiesAsync();

        /// <summary>
        /// Gets the effective capacity for a specific slot (considers both default settings and overrides)
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <returns>Effective capacity and current bookings</returns>
        Task<(int maxCapacity, int currentBookings)> GetEffectiveSlotCapacityAsync(DateTime date, TimeSpan startTime, TimeSpan endTime);

        /// <summary>
        /// Increments or decrements the booking count for a slot
        /// </summary>
        /// <param name="date">Date</param>
        /// <param name="startTime">Start time</param>
        /// <param name="endTime">End time</param>
        /// <param name="increment">Whether to increment (true) or decrement (false)</param>
        Task UpdateSlotBookingCountAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, bool increment);

        /// <summary>
        /// Creates default slot capacities for a specific date based on business hours
        /// </summary>
        /// <param name="date">Date to create slots for</param>
        /// <param name="defaultCapacity">Default capacity per slot</param>
        Task CreateDefaultSlotsForDateAsync(DateTime date, int? defaultCapacity = null);

        /// <summary>
        /// Bulk update slot capacities for a date range
        /// </summary>
        /// <param name="fromDate">From date</param>
        /// <param name="toDate">To date</param>
        /// <param name="startTime">Start time (optional - updates all slots if null)</param>
        /// <param name="endTime">End time (optional - updates all slots if null)</param>
        /// <param name="newCapacity">New capacity</param>
        Task BulkUpdateSlotCapacityAsync(DateTime fromDate, DateTime toDate, TimeSpan? startTime, TimeSpan? endTime, int newCapacity);
    }
}