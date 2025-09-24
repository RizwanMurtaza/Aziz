using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Plugin.Misc.RepairAppointment.Models;
using Nop.Plugin.Misc.RepairAppointment.Models.RepairAppointment;
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
        private readonly IRepairProductService _repairProductService;
        private readonly IRepairTypeService _repairTypeService;
        private readonly ISlotCapacityService _slotCapacityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;

        public RepairAppointmentController(
            IRepairAppointmentService appointmentService,
            IRepairProductService repairProductService,
            IRepairTypeService repairTypeService,
            ISlotCapacityService slotCapacityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService,
            IStoreContext storeContext)
        {
            _appointmentService = appointmentService;
            _repairProductService = repairProductService;
            _repairTypeService = repairTypeService;
            _slotCapacityService = slotCapacityService;
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
                MaxAppointmentsPerSlot = settings.MaxAppointmentsPerSlot,
                BusinessStartTime = settings.BusinessStartTime,
                BusinessEndTime = settings.BusinessEndTime,
                SendConfirmationEmail = settings.SendConfirmationEmail,
                SendReminderEmail = settings.SendReminderEmail,
                ReminderHoursBeforeAppointment = settings.ReminderHoursBeforeAppointment,
                MaxAdvanceBookingDays = settings.MaxAdvanceBookingDays,
                RequireCustomerLogin = settings.RequireCustomerLogin
            };

            // Parse working days from settings
            if (!string.IsNullOrEmpty(settings.WorkingDays))
            {
                var workingDays = settings.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries);
                model.SelectedWorkingDays = workingDays.Where(d => int.TryParse(d, out _))
                                                      .Select(int.Parse)
                                                      .ToList();
            }

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

            // Check if slot duration has changed
            var slotDurationChanged = settings.SlotDurationMinutes != model.SlotDurationMinutes;

            settings.EnableAppointmentSystem = model.EnableAppointmentSystem;
            settings.SlotDurationMinutes = model.SlotDurationMinutes;
            settings.MaxSlotsPerDay = model.MaxSlotsPerDay;
            settings.MaxAppointmentsPerSlot = model.MaxAppointmentsPerSlot;
            settings.BusinessStartTime = model.BusinessStartTime;
            settings.BusinessEndTime = model.BusinessEndTime;
            settings.SendConfirmationEmail = model.SendConfirmationEmail;
            settings.SendReminderEmail = model.SendReminderEmail;
            settings.ReminderHoursBeforeAppointment = model.ReminderHoursBeforeAppointment;
            settings.MaxAdvanceBookingDays = model.MaxAdvanceBookingDays;
            settings.RequireCustomerLogin = model.RequireCustomerLogin;

            // Save working days
            settings.WorkingDays = string.Join(",", model.SelectedWorkingDays);

            // If slot duration changed, delete all existing slot capacity data
            if (slotDurationChanged)
            {
                await _slotCapacityService.DeleteAllSlotCapacitiesAsync();
                _notificationService.WarningNotification("Slot duration changed. All existing slot capacity data has been deleted.");
            }

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

        public async Task<IActionResult> Details(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return RedirectToAction("List");

            // Get product and repair type names
            string? repairProductName = null;
            string? repairTypeName = null;

            if (appointment.RepairProductId.HasValue)
            {
                var product = await _repairProductService.GetRepairProductByIdAsync(appointment.RepairProductId.Value);
                repairProductName = product?.Name;
            }

            if (appointment.RepairTypeId.HasValue)
            {
                var repairType = await _repairTypeService.GetRepairTypeByIdAsync(appointment.RepairTypeId.Value);
                repairTypeName = repairType?.Name;
            }

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
                StatusName = appointment.Status.ToString(),
                Notes = appointment.Notes,
                ConfirmationCode = appointment.ConfirmationCode,
                CreatedOn = appointment.CreatedOnUtc,
                ModifiedOn = appointment.ModifiedOnUtc,
                RepairProductName = repairProductName,
                RepairTypeName = repairTypeName
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/Details.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var appointment = await _appointmentService.GetAppointmentByIdAsync(id);
            if (appointment == null)
                return Json(new { Result = false });

            // Decrement slot booking count before deleting
            if (ParseTimeSlotId(appointment.TimeSlotId, out var slotDate, out var startTime, out var endTime))
            {
                await _slotCapacityService.UpdateSlotBookingCountAsync(
                    appointment.AppointmentDate.Date,
                    startTime,
                    endTime,
                    increment: false);
            }

            await _appointmentService.DeleteAppointmentAsync(appointment);

            return Json(new { Result = true });
        }

        public async Task<IActionResult> TimeSlots()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            return View("~/Plugins/Misc.RepairAppointment/Views/TimeSlots.cshtml");
        }

        private bool ParseTimeSlotId(string timeSlotId, out DateTime date, out TimeSpan startTime, out TimeSpan endTime)
        {
            date = DateTime.MinValue;
            startTime = TimeSpan.Zero;
            endTime = TimeSpan.Zero;

            if (string.IsNullOrEmpty(timeSlotId))
                return false;

            // Expected format: "2025-09-29_13:00_13:30"
            var parts = timeSlotId.Split('_');
            if (parts.Length != 3)
                return false;

            // Parse date
            if (!DateTime.TryParseExact(parts[0], "yyyy-MM-dd", null, System.Globalization.DateTimeStyles.None, out date))
                return false;

            // Parse start time
            if (!TimeSpan.TryParseExact(parts[1], @"hh\:mm", null, out startTime))
                return false;

            // Parse end time
            if (!TimeSpan.TryParseExact(parts[2], @"hh\:mm", null, out endTime))
                return false;

            return true;
        }
    }
}