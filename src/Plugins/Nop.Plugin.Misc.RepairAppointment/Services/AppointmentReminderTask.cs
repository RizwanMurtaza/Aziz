using System;
using System.Linq;
using System.Threading.Tasks;
using Nop.Core.Domain.Messages;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.ScheduleTasks;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public class AppointmentReminderTask : IScheduleTask
    {
        private readonly IRepairAppointmentService _appointmentService;
        private readonly ISettingService _settingService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly ILocalizationService _localizationService;
        private readonly EmailAccountSettings _emailAccountSettings;

        public AppointmentReminderTask(
            IRepairAppointmentService appointmentService,
            ISettingService settingService,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            ILocalizationService localizationService,
            EmailAccountSettings emailAccountSettings)
        {
            _appointmentService = appointmentService;
            _settingService = settingService;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _localizationService = localizationService;
            _emailAccountSettings = emailAccountSettings;
        }

        public async Task ExecuteAsync()
        {
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();

            if (!settings.SendReminderEmail)
                return;

            var appointments = await _appointmentService.GetUpcomingAppointmentsForReminderAsync(settings.ReminderHoursBeforeAppointment);

            foreach (var appointment in appointments)
            {
                await SendReminderEmailAsync(appointment);
            }
        }

        private async Task SendReminderEmailAsync(Domain.RepairAppointment appointment)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

            if (emailAccount == null)
                return;

            var subject = await _localizationService.GetResourceAsync("Plugins.Misc.RepairAppointment.ReminderEmailSubject");

            var body = $@"
                <h2>Repair Appointment Reminder</h2>
                <p>Dear {appointment.CustomerName},</p>
                <p>This is a reminder of your upcoming repair appointment:</p>
                <ul>
                    <li><strong>Confirmation Code:</strong> {appointment.ConfirmationCode}</li>
                    <li><strong>Date:</strong> {appointment.AppointmentDate:dddd, MMMM dd, yyyy}</li>
                    <li><strong>Time:</strong> {appointment.TimeSlot}</li>
                    <li><strong>Device:</strong> {appointment.DeviceType} - {appointment.DeviceBrand} {appointment.DeviceModel}</li>
                </ul>
                <p>Please arrive 5-10 minutes before your appointment time.</p>
                <p>If you need to cancel or reschedule, please contact us immediately.</p>
                <p>Thank you!</p>
            ";

            await _queuedEmailService.InsertQueuedEmailAsync(new QueuedEmail
            {
                From = emailAccount.Email,
                FromName = emailAccount.DisplayName,
                To = appointment.Email,
                ToName = appointment.CustomerName,
                Subject = subject,
                Body = body,
                CreatedOnUtc = DateTime.UtcNow,
                EmailAccountId = emailAccount.Id
            });

            appointment.ReminderSent = true;
            await _appointmentService.UpdateAppointmentAsync(appointment);
        }
    }
}