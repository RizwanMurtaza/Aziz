using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Services.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    /// <summary>
    /// Service for managing slot capacity overrides
    /// </summary>
    public class SlotCapacityService : ISlotCapacityService
    {
        private readonly IRepository<SlotCapacity> _slotCapacityRepository;
        private readonly IRepository<Domain.RepairAppointment> _appointmentRepository;
        private readonly ISettingService _settingService;

        public SlotCapacityService(
            IRepository<SlotCapacity> slotCapacityRepository,
            IRepository<Domain.RepairAppointment> appointmentRepository,
            ISettingService settingService)
        {
            _slotCapacityRepository = slotCapacityRepository;
            _appointmentRepository = appointmentRepository;
            _settingService = settingService;
        }

        public virtual async Task<SlotCapacity?> GetSlotCapacityByIdAsync(int id)
        {
            return await _slotCapacityRepository.GetByIdAsync(id, cache => default);
        }

        public virtual async Task<SlotCapacity?> GetSlotCapacityAsync(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var dateOnly = date.Date;
            return await _slotCapacityRepository.Table
                .Where(sc => sc.Date == dateOnly && sc.StartTime == startTime && sc.EndTime == endTime)
                .FirstOrDefaultAsync();
        }

        public virtual async Task<IList<SlotCapacity>> GetSlotCapacitiesByDateAsync(DateTime date, bool activeOnly = true)
        {
            var dateOnly = date.Date;
            var query = _slotCapacityRepository.Table.Where(sc => sc.Date == dateOnly);

            if (activeOnly)
                query = query.Where(sc => sc.IsActive);

            return await query.OrderBy(sc => sc.StartTime).ToListAsync();
        }

        public virtual async Task<IPagedList<SlotCapacity>> GetSlotCapacitiesAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            bool? activeOnly = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _slotCapacityRepository.Table.AsQueryable();

            if (fromDate.HasValue)
                query = query.Where(sc => sc.Date >= fromDate.Value.Date);

            if (toDate.HasValue)
                query = query.Where(sc => sc.Date <= toDate.Value.Date);

            if (activeOnly.HasValue)
                query = query.Where(sc => sc.IsActive == activeOnly.Value);

            query = query.OrderBy(sc => sc.Date).ThenBy(sc => sc.StartTime);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public virtual async Task InsertSlotCapacityAsync(SlotCapacity slotCapacity)
        {
            ArgumentNullException.ThrowIfNull(slotCapacity);

            slotCapacity.Date = slotCapacity.Date.Date; // Ensure only date part
            slotCapacity.CreatedOnUtc = DateTime.UtcNow;

            await _slotCapacityRepository.InsertAsync(slotCapacity);
        }

        public virtual async Task UpdateSlotCapacityAsync(SlotCapacity slotCapacity)
        {
            ArgumentNullException.ThrowIfNull(slotCapacity);

            slotCapacity.ModifiedOnUtc = DateTime.UtcNow;

            await _slotCapacityRepository.UpdateAsync(slotCapacity);
        }

        public virtual async Task DeleteSlotCapacityAsync(SlotCapacity slotCapacity)
        {
            ArgumentNullException.ThrowIfNull(slotCapacity);

            await _slotCapacityRepository.DeleteAsync(slotCapacity);
        }

        public virtual async Task<(int maxCapacity, int currentBookings)> GetEffectiveSlotCapacityAsync(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            // Always get the actual current bookings from the appointments table for accuracy
            var currentBookings = await GetCurrentBookingsForSlotAsync(date, startTime, endTime);

            // Check for specific slot capacity override first
            var slotCapacity = await GetSlotCapacityAsync(date, startTime, endTime);
            if (slotCapacity != null && slotCapacity.IsActive)
            {
                return (slotCapacity.MaxAppointments, currentBookings);
            }

            // Fall back to default settings
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();
            var defaultCapacity = settings.MaxAppointmentsPerSlot;

            return (defaultCapacity, currentBookings);
        }

        public virtual async Task UpdateSlotBookingCountAsync(DateTime date, TimeSpan startTime, TimeSpan endTime, bool increment)
        {
            var slotCapacity = await GetSlotCapacityAsync(date, startTime, endTime);

            if (slotCapacity != null)
            {
                // Update existing slot capacity record
                slotCapacity.CurrentBookings = increment
                    ? slotCapacity.CurrentBookings + 1
                    : Math.Max(0, slotCapacity.CurrentBookings - 1);

                await UpdateSlotCapacityAsync(slotCapacity);
            }
            else
            {
                // Create new slot capacity record if one doesn't exist and we're incrementing
                if (increment)
                {
                    var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();
                    slotCapacity = new SlotCapacity
                    {
                        Date = date.Date,
                        StartTime = startTime,
                        EndTime = endTime,
                        MaxAppointments = settings.MaxAppointmentsPerSlot,
                        CurrentBookings = 1,
                        IsActive = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await InsertSlotCapacityAsync(slotCapacity);
                }
            }
        }

        public virtual async Task CreateDefaultSlotsForDateAsync(DateTime date, int? defaultCapacity = null)
        {
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();
            var capacity = defaultCapacity ?? settings.MaxAppointmentsPerSlot;

            // Parse business hours
            if (!TimeSpan.TryParse(settings.BusinessStartTime, out var startTime) ||
                !TimeSpan.TryParse(settings.BusinessEndTime, out var endTime))
            {
                return; // Invalid time format
            }

            var slotDuration = TimeSpan.FromMinutes(settings.SlotDurationMinutes);
            var currentTime = startTime;
            var dateOnly = date.Date;

            var existingSlots = await GetSlotCapacitiesByDateAsync(dateOnly, false);
            var existingSlotTimes = existingSlots.Select(s => s.StartTime).ToHashSet();

            while (currentTime.Add(slotDuration) <= endTime)
            {
                var slotEndTime = currentTime.Add(slotDuration);

                // Only create slot if it doesn't already exist
                if (!existingSlotTimes.Contains(currentTime))
                {
                    var slotCapacity = new SlotCapacity
                    {
                        Date = dateOnly,
                        StartTime = currentTime,
                        EndTime = slotEndTime,
                        MaxAppointments = capacity,
                        CurrentBookings = 0,
                        IsActive = true,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await InsertSlotCapacityAsync(slotCapacity);
                }

                currentTime = currentTime.Add(slotDuration);
            }
        }

        public virtual async Task BulkUpdateSlotCapacityAsync(DateTime fromDate, DateTime toDate, TimeSpan? startTime, TimeSpan? endTime, int newCapacity)
        {
            var query = _slotCapacityRepository.Table
                .Where(sc => sc.Date >= fromDate.Date && sc.Date <= toDate.Date);

            if (startTime.HasValue)
                query = query.Where(sc => sc.StartTime == startTime.Value);

            if (endTime.HasValue)
                query = query.Where(sc => sc.EndTime == endTime.Value);

            var slots = await query.ToListAsync();

            foreach (var slot in slots)
            {
                slot.MaxAppointments = newCapacity;
                slot.ModifiedOnUtc = DateTime.UtcNow;
            }

            await _slotCapacityRepository.UpdateAsync(slots);
        }

        /// <summary>
        /// Helper method to get current bookings from appointments table
        /// This queries the actual appointments to get real booking count
        /// </summary>
        private async Task<int> GetCurrentBookingsForSlotAsync(DateTime date, TimeSpan startTime, TimeSpan endTime)
        {
            var dateOnly = date.Date;
            var appointmentDateTime = dateOnly.Add(startTime);
            var endDateTime = dateOnly.Add(endTime);

            // Count active appointments (not cancelled) for this specific date and time slot
            var count = await _appointmentRepository.Table
                .Where(a => a.AppointmentDate >= appointmentDateTime
                           && a.AppointmentDate < endDateTime
                           && a.Status != AppointmentStatus.Cancelled)
                .CountAsync();

            return count;
        }
    }
}