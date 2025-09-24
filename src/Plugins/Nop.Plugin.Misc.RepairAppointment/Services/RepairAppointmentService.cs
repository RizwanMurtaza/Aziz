using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Data;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Services.Configuration;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public class RepairAppointmentService : IRepairAppointmentService
    {
        private readonly IRepository<Domain.RepairAppointment> _appointmentRepository;
        private readonly IRepository<RepairTimeSlot> _timeSlotRepository;
        private readonly ISettingService _settingService;

        public RepairAppointmentService(
            IRepository<Domain.RepairAppointment> appointmentRepository,
            IRepository<RepairTimeSlot> timeSlotRepository,
            ISettingService settingService)
        {
            _appointmentRepository = appointmentRepository;
            _timeSlotRepository = timeSlotRepository;
            _settingService = settingService;
        }

        public async Task<Domain.RepairAppointment?> GetAppointmentByIdAsync(int appointmentId)
        {
            if (appointmentId == 0)
                return null;

            return await _appointmentRepository.GetByIdAsync(appointmentId);
        }

        public async Task<Domain.RepairAppointment?> GetAppointmentByConfirmationCodeAsync(string confirmationCode)
        {
            if (string.IsNullOrEmpty(confirmationCode))
                return null;

            var query = _appointmentRepository.Table
                .Where(a => a.ConfirmationCode == confirmationCode);

            return await query.FirstOrDefaultAsync();
        }

        public async Task<IPagedList<Domain.RepairAppointment>> GetAllAppointmentsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            AppointmentStatus? status = null,
            string? searchText = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue)
        {
            var query = _appointmentRepository.Table;

            if (fromDate.HasValue)
                query = query.Where(a => a.AppointmentDate >= fromDate.Value);

            if (toDate.HasValue)
                query = query.Where(a => a.AppointmentDate <= toDate.Value);

            if (status.HasValue)
                query = query.Where(a => a.Status == status.Value);

            if (!string.IsNullOrEmpty(searchText))
            {
                query = query.Where(a =>
                    a.CustomerName.Contains(searchText) ||
                    a.Email.Contains(searchText) ||
                    a.Phone.Contains(searchText) ||
                    a.DeviceModel.Contains(searchText) ||
                    a.ConfirmationCode.Contains(searchText));
            }

            query = query.OrderByDescending(a => a.AppointmentDate);

            return await query.ToPagedListAsync(pageIndex, pageSize);
        }

        public async Task InsertAppointmentAsync(Domain.RepairAppointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            appointment.CreatedOnUtc = DateTime.UtcNow;
            appointment.ConfirmationCode = await GenerateConfirmationCodeAsync();

            await _appointmentRepository.InsertAsync(appointment);
            await UpdateSlotBookingCountAsync(appointment.TimeSlotId, true);
        }

        public async Task UpdateAppointmentAsync(Domain.RepairAppointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            appointment.ModifiedOnUtc = DateTime.UtcNow;
            await _appointmentRepository.UpdateAsync(appointment);
        }

        public async Task DeleteAppointmentAsync(Domain.RepairAppointment appointment)
        {
            if (appointment == null)
                throw new ArgumentNullException(nameof(appointment));

            await UpdateSlotBookingCountAsync(appointment.TimeSlotId, false);
            await _appointmentRepository.DeleteAsync(appointment);
        }

        public async Task<bool> IsSlotAvailableAsync(DateTime date, int timeSlotId)
        {
            var slot = await GetTimeSlotByIdAsync(timeSlotId);
            if (slot == null)
                return false;

            return slot.IsAvailable && !slot.IsBlocked &&
                   slot.CurrentBookings < slot.MaxAppointments;
        }

        public async Task<List<RepairTimeSlot>> GetAvailableSlotsForDateAsync(DateTime date)
        {
            var dateOnly = date.Date;

            var slots = await _timeSlotRepository.Table
                .Where(s => s.Date == dateOnly && s.IsAvailable && !s.IsBlocked)
                .OrderBy(s => s.StartTime)
                .ToListAsync();

            if (!slots.Any())
            {
                await GenerateTimeSlotsForDateAsync(dateOnly);

                slots = await _timeSlotRepository.Table
                    .Where(s => s.Date == dateOnly && s.IsAvailable && !s.IsBlocked)
                    .OrderBy(s => s.StartTime)
                    .ToListAsync();
            }

            return slots.Where(s => s.CurrentBookings < s.MaxAppointments).ToList();
        }

        public async Task<RepairTimeSlot?> GetTimeSlotByIdAsync(int timeSlotId)
        {
            if (timeSlotId == 0)
                return null;

            return await _timeSlotRepository.GetByIdAsync(timeSlotId);
        }

        public async Task GenerateTimeSlotsForDateAsync(DateTime date)
        {
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();
            var dateOnly = date.Date;

            var existingSlots = await _timeSlotRepository.Table
                .Where(s => s.Date == dateOnly)
                .ToListAsync();

            if (existingSlots.Any())
                return;

            var startTime = TimeSpan.Parse(settings.BusinessStartTime);
            var endTime = TimeSpan.Parse(settings.BusinessEndTime);
            var slotDuration = TimeSpan.FromMinutes(settings.SlotDurationMinutes);

            var currentTime = startTime;
            while (currentTime < endTime)
            {
                var slot = new RepairTimeSlot
                {
                    Date = dateOnly,
                    StartTime = currentTime,
                    EndTime = currentTime.Add(slotDuration),
                    TimeSlot = $"{currentTime:hh\\:mm} - {currentTime.Add(slotDuration):hh\\:mm}",
                    MaxAppointments = 1,
                    CurrentBookings = 0,
                    IsAvailable = true,
                    IsBlocked = false
                };

                await _timeSlotRepository.InsertAsync(slot);
                currentTime = currentTime.Add(slotDuration);
            }
        }

        public async Task BlockTimeSlotAsync(int timeSlotId, string reason)
        {
            var slot = await GetTimeSlotByIdAsync(timeSlotId);
            if (slot != null)
            {
                slot.IsBlocked = true;
                slot.BlockReason = reason;
                await _timeSlotRepository.UpdateAsync(slot);
            }
        }

        public async Task UnblockTimeSlotAsync(int timeSlotId)
        {
            var slot = await GetTimeSlotByIdAsync(timeSlotId);
            if (slot != null)
            {
                slot.IsBlocked = false;
                slot.BlockReason = null;
                await _timeSlotRepository.UpdateAsync(slot);
            }
        }

        public async Task<string> GenerateConfirmationCodeAsync()
        {
            var random = new Random();
            string code;
            bool exists;

            do
            {
                code = "RA" + random.Next(100000, 999999).ToString();
                var existingAppointment = await GetAppointmentByConfirmationCodeAsync(code);
                exists = existingAppointment != null;
            }
            while (exists);

            return code;
        }

        public async Task<IList<Domain.RepairAppointment>> GetUpcomingAppointmentsForReminderAsync(int hoursBeforeAppointment)
        {
            var reminderTime = DateTime.UtcNow.AddHours(hoursBeforeAppointment);

            var appointments = await _appointmentRepository.Table
                .Where(a => a.Status == AppointmentStatus.Confirmed &&
                           !a.ReminderSent &&
                           a.AppointmentDate >= reminderTime &&
                           a.AppointmentDate <= reminderTime.AddHours(1))
                .ToListAsync();

            return appointments;
        }

        public async Task UpdateSlotBookingCountAsync(int timeSlotId, bool increment)
        {
            try
            {
                var slot = await GetTimeSlotByIdAsync(timeSlotId);
                if (slot != null)
                {
                    // Check if the TimeSpan values are corrupted
                    if (slot.StartTime.TotalHours >= 24 || slot.EndTime.TotalHours >= 24 ||
                        slot.StartTime < TimeSpan.Zero || slot.EndTime < TimeSpan.Zero)
                    {
                        // Skip the update if TimeSpan values are corrupted to prevent MySQL error
                        System.Diagnostics.Debug.WriteLine($"Skipping slot update due to corrupted TimeSpan values: StartTime={slot.StartTime}, EndTime={slot.EndTime}");
                        return;
                    }

                    // Update booking count
                    if (increment)
                    {
                        if (slot.CurrentBookings < slot.MaxAppointments)
                            slot.CurrentBookings++;
                    }
                    else if (slot.CurrentBookings > 0)
                    {
                        slot.CurrentBookings--;
                    }

                    await _timeSlotRepository.UpdateAsync(slot);
                }
            }
            catch (Exception ex)
            {
                // Log the error but don't break the appointment creation
                System.Diagnostics.Debug.WriteLine($"Error updating slot booking count: {ex.Message}");

                // Don't rethrow to avoid breaking appointment creation
                // In production, you would log this properly and possibly alert administrators
            }
        }
    }
}