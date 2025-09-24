using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Plugin.Misc.RepairAppointment.Models;
using Nop.Plugin.Misc.RepairAppointment.Services;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;

namespace Nop.Plugin.Misc.RepairAppointment.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class RepairAppointmentController : BasePluginController
    {
        private readonly IRepairAppointmentService _appointmentService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public RepairAppointmentController(
            IRepairAppointmentService appointmentService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _appointmentService = appointmentService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
            _storeContext = storeContext;
        }

        public async Task<IActionResult> Configure()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>(storeScope);

            var model = new ConfigurationModel
            {
                EnableAppointmentSystem = settings.EnableAppointmentSystem,
                SlotDurationMinutes = settings.SlotDurationMinutes,
                MaxSlotsPerDay = settings.MaxSlotsPerDay,
                BusinessStartTime = settings.BusinessStartTime,
                BusinessEndTime = settings.BusinessEndTime,
                SendConfirmationEmail = settings.SendConfirmationEmail,
                SendReminderEmail = settings.SendReminderEmail,
                ReminderHoursBeforeAppointment = settings.ReminderHoursBeforeAppointment,
                MaxAdvanceBookingDays = settings.MaxAdvanceBookingDays,
                RequireCustomerLogin = settings.RequireCustomerLogin
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/Configure.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Configure(ConfigurationModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return await Configure();

            var storeScope = await _storeContext.GetActiveStoreScopeConfigurationAsync();
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>(storeScope);

            settings.EnableAppointmentSystem = model.EnableAppointmentSystem;
            settings.SlotDurationMinutes = model.SlotDurationMinutes;
            settings.MaxSlotsPerDay = model.MaxSlotsPerDay;
            settings.BusinessStartTime = model.BusinessStartTime;
            settings.BusinessEndTime = model.BusinessEndTime;
            settings.SendConfirmationEmail = model.SendConfirmationEmail;
            settings.SendReminderEmail = model.SendReminderEmail;
            settings.ReminderHoursBeforeAppointment = model.ReminderHoursBeforeAppointment;
            settings.MaxAdvanceBookingDays = model.MaxAdvanceBookingDays;
            settings.RequireCustomerLogin = model.RequireCustomerLogin;

            await _settingService.SaveSettingAsync(settings, storeScope);

            _notificationService.SuccessNotification(await _localizationService.GetResourceAsync("Admin.Plugins.Saved"));

            return await Configure();
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new RepairAppointmentSearchModel();
            model.SetGridPageSize();

            model.AvailableStatuses = Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = ((int)s).ToString()
                })
                .ToList();

            model.AvailableStatuses.Insert(0, new SelectListItem { Text = await _localizationService.GetResourceAsync("Admin.Common.All"), Value = "0" });

            return View("~/Plugins/Misc.RepairAppointment/Views/List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> AppointmentList(RepairAppointmentSearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var status = searchModel.StatusId > 0 ? (AppointmentStatus?)searchModel.StatusId : null;

            var appointments = await _appointmentService.GetAllAppointmentsAsync(
                fromDate: searchModel.FromDate,
                toDate: searchModel.ToDate,
                status: status,
                searchText: searchModel.SearchText,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = new RepairAppointmentListModel
            {
                Data = appointments.Select(appointment => new RepairAppointmentModel
                {
                    Id = appointment.Id,
                    CustomerName = appointment.CustomerName,
                    Email = appointment.Email,
                    Phone = appointment.Phone,
                    DeviceType = appointment.DeviceType,
                    DeviceBrand = appointment.DeviceBrand,
                    DeviceModel = appointment.DeviceModel,
                    IssueDescription = appointment.IssueDescription,
                    AppointmentDate = appointment.AppointmentDate,
                    TimeSlot = appointment.TimeSlot,
                    StatusId = (int)appointment.Status,
                    StatusName = appointment.Status.ToString(),
                    ConfirmationCode = appointment.ConfirmationCode,
                    CreatedOn = appointment.CreatedOnUtc.ToLocalTime()
                }).ToList(),
                Draw = searchModel.Draw,
                RecordsTotal = appointments.TotalCount,
                RecordsFiltered = appointments.TotalCount
            };

            return Json(model);
        }

        [Route("Admin/RepairAppointment/Edit/{id:int}")]
        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return RedirectToAction("List");

            var model = new RepairAppointmentModel
            {
                Id = appointment.Id,
                CustomerName = appointment.CustomerName,
                Email = appointment.Email,
                Phone = appointment.Phone,
                DeviceType = appointment.DeviceType,
                DeviceBrand = appointment.DeviceBrand,
                DeviceModel = appointment.DeviceModel,
                IssueDescription = appointment.IssueDescription,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = appointment.TimeSlot,
                StatusId = (int)appointment.Status,
                Notes = appointment.Notes,
                ConfirmationCode = appointment.ConfirmationCode
            };

            model.AvailableStatuses = Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = ((int)s).ToString(),
                    Selected = (int)s == model.StatusId
                })
                .ToList();

            return View("~/Plugins/Misc.RepairAppointment/Views/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(RepairAppointmentModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(model.Id);
            if (appointment == null)
                return RedirectToAction("List");

            if (ModelState.IsValid)
            {
                appointment.CustomerName = model.CustomerName;
                appointment.Email = model.Email;
                appointment.Phone = model.Phone;
                appointment.DeviceType = model.DeviceType;
                appointment.DeviceBrand = model.DeviceBrand;
                appointment.DeviceModel = model.DeviceModel;
                appointment.IssueDescription = model.IssueDescription;
                appointment.Status = (AppointmentStatus)model.StatusId;
                appointment.Notes = model.Notes;

                await _appointmentService.UpdateAppointmentAsync(appointment);

                _notificationService.SuccessNotification("Appointment updated successfully");

                return RedirectToAction("List");
            }

            model.AvailableStatuses = Enum.GetValues(typeof(AppointmentStatus))
                .Cast<AppointmentStatus>()
                .Select(s => new SelectListItem
                {
                    Text = s.ToString(),
                    Value = ((int)s).ToString(),
                    Selected = (int)s == model.StatusId
                })
                .ToList();

            return View("~/Plugins/Misc.RepairAppointment/Views/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return Json(new { Result = false });

            await _appointmentService.DeleteAppointmentAsync(appointment);

            return Json(new { Result = true });
        }

        public async Task<IActionResult> TimeSlots()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            return View("~/Plugins/Misc.RepairAppointment/Views/TimeSlots.cshtml");
        }
    }
}