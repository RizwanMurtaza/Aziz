using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Core;
using Nop.Plugin.Misc.RepairAppointment.Domain;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public interface IRepairAppointmentService
    {
        Task<Domain.RepairAppointment?> GetAppointmentByIdAsync(int appointmentId);
        Task<Domain.RepairAppointment?> GetAppointmentByConfirmationCodeAsync(string confirmationCode);
        Task<IPagedList<Domain.RepairAppointment>> GetAllAppointmentsAsync(
            DateTime? fromDate = null,
            DateTime? toDate = null,
            AppointmentStatus? status = null,
            string? searchText = null,
            int pageIndex = 0,
            int pageSize = int.MaxValue);
        Task InsertAppointmentAsync(Domain.RepairAppointment appointment);
        Task UpdateAppointmentAsync(Domain.RepairAppointment appointment);
        Task DeleteAppointmentAsync(Domain.RepairAppointment appointment);
        Task<bool> IsSlotAvailableAsync(DateTime date, int timeSlotId);
        Task<List<RepairTimeSlot>> GetAvailableSlotsForDateAsync(DateTime date);
        Task<RepairTimeSlot?> GetTimeSlotByIdAsync(int timeSlotId);
        Task GenerateTimeSlotsForDateAsync(DateTime date);
        Task BlockTimeSlotAsync(int timeSlotId, string reason);
        Task UnblockTimeSlotAsync(int timeSlotId);
        Task<string> GenerateConfirmationCodeAsync();
        Task<IList<Domain.RepairAppointment>> GetUpcomingAppointmentsForReminderAsync(int hoursBeforeAppointment);
        Task UpdateSlotBookingCountAsync(int timeSlotId, bool increment);
    }
}