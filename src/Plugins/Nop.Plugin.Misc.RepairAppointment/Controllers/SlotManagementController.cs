using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.RepairAppointment.Models;
using Nop.Plugin.Misc.RepairAppointment.Services;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Services.Security;
using Nop.Web.Framework;
using Nop.Web.Framework.Controllers;
using Nop.Web.Framework.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RepairAppointment.Controllers
{
    [AuthorizeAdmin]
    [Area(AreaNames.ADMIN)]
    [AutoValidateAntiforgeryToken]
    public class SlotManagementController : BasePluginController
    {
        private readonly ISlotCapacityService _slotCapacityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly IPermissionService _permissionService;
        private readonly ISettingService _settingService;

        public SlotManagementController(
            ISlotCapacityService slotCapacityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            IPermissionService permissionService,
            ISettingService settingService)
        {
            _slotCapacityService = slotCapacityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _permissionService = permissionService;
            _settingService = settingService;
        }

        public async Task<IActionResult> List()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new SlotCapacitySearchModel();
            model.SetGridPageSize();

            return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/List.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> SlotCapacityList(SlotCapacitySearchModel searchModel)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var slotCapacities = await _slotCapacityService.GetSlotCapacitiesAsync(
                fromDate: searchModel.FromDate,
                toDate: searchModel.ToDate,
                activeOnly: searchModel.IsActive,
                pageIndex: searchModel.Page - 1,
                pageSize: searchModel.PageSize);

            var model = new SlotCapacityListModel
            {
                Data = slotCapacities.Select(slotCapacity => new SlotCapacityModel
                {
                    Id = slotCapacity.Id,
                    Date = slotCapacity.Date,
                    StartTime = slotCapacity.StartTime.ToString(@"hh\:mm"),
                    EndTime = slotCapacity.EndTime.ToString(@"hh\:mm"),
                    MaxAppointments = slotCapacity.MaxAppointments,
                    CurrentBookings = slotCapacity.CurrentBookings,
                    IsActive = slotCapacity.IsActive,
                    Notes = slotCapacity.Notes,
                    CreatedOn = slotCapacity.CreatedOnUtc,
                    ModifiedOn = slotCapacity.ModifiedOnUtc
                }).ToList(),
                Draw = searchModel.Draw,
                RecordsTotal = slotCapacities.TotalCount,
                RecordsFiltered = slotCapacities.TotalCount
            };

            return Json(model);
        }

        public async Task<IActionResult> Create()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new SlotCapacityModel
            {
                Date = DateTime.Today.AddDays(1), // Default to tomorrow
                MaxAppointments = 1,
                IsActive = true
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Create.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Create(SlotCapacityModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            if (!ModelState.IsValid)
                return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Create.cshtml", model);

            // Parse time strings
            if (!TimeSpan.TryParse(model.StartTime, out var startTime) ||
                !TimeSpan.TryParse(model.EndTime, out var endTime))
            {
                ModelState.AddModelError("", "Invalid time format");
                return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Create.cshtml", model);
            }

            if (startTime >= endTime)
            {
                ModelState.AddModelError("", "Start time must be before end time");
                return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Create.cshtml", model);
            }

            // Check if slot already exists
            var existingSlot = await _slotCapacityService.GetSlotCapacityAsync(model.Date, startTime, endTime);
            if (existingSlot != null)
            {
                ModelState.AddModelError("", "A slot with this date and time already exists");
                return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Create.cshtml", model);
            }

            var slotCapacity = new SlotCapacity
            {
                Date = model.Date.Date,
                StartTime = startTime,
                EndTime = endTime,
                MaxAppointments = model.MaxAppointments,
                CurrentBookings = 0,
                IsActive = model.IsActive,
                Notes = model.Notes
            };

            await _slotCapacityService.InsertSlotCapacityAsync(slotCapacity);

            _notificationService.SuccessNotification("Slot capacity created successfully");

            return RedirectToAction("List");
        }

        public async Task<IActionResult> Edit(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var slotCapacity = await _slotCapacityService.GetSlotCapacityByIdAsync(id);
            if (slotCapacity == null)
                return RedirectToAction("List");

            var model = new SlotCapacityModel
            {
                Id = slotCapacity.Id,
                Date = slotCapacity.Date,
                StartTime = slotCapacity.StartTime.ToString(@"hh\:mm"),
                EndTime = slotCapacity.EndTime.ToString(@"hh\:mm"),
                MaxAppointments = slotCapacity.MaxAppointments,
                CurrentBookings = slotCapacity.CurrentBookings,
                IsActive = slotCapacity.IsActive,
                Notes = slotCapacity.Notes,
                CreatedOn = slotCapacity.CreatedOnUtc,
                ModifiedOn = slotCapacity.ModifiedOnUtc
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Edit.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(SlotCapacityModel model)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var slotCapacity = await _slotCapacityService.GetSlotCapacityByIdAsync(model.Id);
            if (slotCapacity == null)
                return RedirectToAction("List");

            if (!ModelState.IsValid)
                return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Edit.cshtml", model);

            // Parse time strings
            if (!TimeSpan.TryParse(model.StartTime, out var startTime) ||
                !TimeSpan.TryParse(model.EndTime, out var endTime))
            {
                ModelState.AddModelError("", "Invalid time format");
                return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Edit.cshtml", model);
            }

            if (startTime >= endTime)
            {
                ModelState.AddModelError("", "Start time must be before end time");
                return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/Edit.cshtml", model);
            }

            // Update properties
            slotCapacity.Date = model.Date.Date;
            slotCapacity.StartTime = startTime;
            slotCapacity.EndTime = endTime;
            slotCapacity.MaxAppointments = model.MaxAppointments;
            slotCapacity.IsActive = model.IsActive;
            slotCapacity.Notes = model.Notes;

            await _slotCapacityService.UpdateSlotCapacityAsync(slotCapacity);

            _notificationService.SuccessNotification("Slot capacity updated successfully");

            return RedirectToAction("List");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return await AccessDeniedJsonAsync();

            var slotCapacity = await _slotCapacityService.GetSlotCapacityByIdAsync(id);
            if (slotCapacity == null)
                return Json(new { Result = false, Message = "Slot not found" });

            if (slotCapacity.CurrentBookings > 0)
                return Json(new { Result = false, Message = "Cannot delete slot with existing bookings" });

            await _slotCapacityService.DeleteSlotCapacityAsync(slotCapacity);

            return Json(new { Result = true });
        }

        public async Task<IActionResult> BulkManagement()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return AccessDeniedView();

            var model = new BulkSlotManagementModel
            {
                FromDate = DateTime.Today.AddDays(1),
                ToDate = DateTime.Today.AddDays(7)
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/SlotManagement/BulkManagement.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> GetTimeSlotsForWeekdays([FromBody] GetTimeSlotsRequest request)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return Json(new { success = false, message = "Access denied" });

            try
            {
                var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();
                var timeSlots = new List<TimeSlotCapacityModel>();

                if (!TimeSpan.TryParse(settings.BusinessStartTime, out var startTime) ||
                    !TimeSpan.TryParse(settings.BusinessEndTime, out var endTime))
                {
                    return Json(new { success = false, message = "Invalid business hours configuration" });
                }

                var slotDuration = TimeSpan.FromMinutes(settings.SlotDurationMinutes);
                var currentTime = startTime;

                while (currentTime.Add(slotDuration) <= endTime)
                {
                    var slotEndTime = currentTime.Add(slotDuration);

                    timeSlots.Add(new TimeSlotCapacityModel
                    {
                        TimeSlot = $"{currentTime:hh\\:mm} - {slotEndTime:hh\\:mm}",
                        StartTime = currentTime,
                        EndTime = slotEndTime,
                        DefaultCapacity = settings.MaxAppointmentsPerSlot,
                        NewCapacity = settings.MaxAppointmentsPerSlot
                    });

                    currentTime = currentTime.Add(slotDuration);
                }

                return Json(new { success = true, timeSlots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ApplyBulkCapacityChanges([FromBody] ApplyBulkChangesRequest request)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return Json(new { success = false, message = "Access denied" });

            try
            {
                int affectedSlots = 0;
                var currentDate = request.FromDate;

                // Get working days from settings for validation
                var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();
                var workingDays = new List<int>();
                if (!string.IsNullOrEmpty(settings.WorkingDays))
                {
                    workingDays = settings.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(int.Parse)
                                                     .ToList();
                }

                while (currentDate <= request.ToDate)
                {
                    // Check if current date's day of week is in selected weekdays AND is a working day
                    var dayOfWeek = (int)currentDate.DayOfWeek;
                    if (request.SelectedWeekdays.Contains(dayOfWeek) &&
                        (workingDays.Count == 0 || workingDays.Contains(dayOfWeek)))
                    {
                        foreach (var timeSlot in request.TimeSlots)
                        {
                            if (timeSlot.NewCapacity != timeSlot.DefaultCapacity)
                            {
                                // Create or update slot capacity for this specific date and time
                                var existingSlot = await _slotCapacityService.GetSlotCapacityAsync(
                                    currentDate, timeSlot.StartTime, timeSlot.EndTime);

                                if (existingSlot != null)
                                {
                                    existingSlot.MaxAppointments = timeSlot.NewCapacity;
                                    existingSlot.ModifiedOnUtc = DateTime.UtcNow;
                                    await _slotCapacityService.UpdateSlotCapacityAsync(existingSlot);
                                }
                                else
                                {
                                    // Create new slot capacity entry
                                    var newSlotCapacity = new Domain.SlotCapacity
                                    {
                                        Date = currentDate,
                                        StartTime = timeSlot.StartTime,
                                        EndTime = timeSlot.EndTime,
                                        MaxAppointments = timeSlot.NewCapacity,
                                        CurrentBookings = 0,
                                        IsActive = true,
                                        CreatedOnUtc = DateTime.UtcNow
                                    };
                                    await _slotCapacityService.InsertSlotCapacityAsync(newSlotCapacity);
                                }
                                affectedSlots++;
                            }
                        }
                    }
                    currentDate = currentDate.AddDays(1);
                }

                return Json(new { success = true, affectedSlots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAvailableTimeSlots()
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return Json(new { success = false, message = "Access denied" });

            try
            {
                var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();
                var timeSlots = new List<object>();

                if (!TimeSpan.TryParse(settings.BusinessStartTime, out var startTime) ||
                    !TimeSpan.TryParse(settings.BusinessEndTime, out var endTime))
                {
                    return Json(new { success = false, message = "Invalid business hours configuration" });
                }

                var slotDuration = TimeSpan.FromMinutes(settings.SlotDurationMinutes);
                var currentTime = startTime;

                while (currentTime.Add(slotDuration) <= endTime)
                {
                    var slotEndTime = currentTime.Add(slotDuration);

                    timeSlots.Add(new
                    {
                        timeSlot = $"{currentTime:hh\\:mm} - {slotEndTime:hh\\:mm}",
                        startTime = currentTime.ToString(@"hh\:mm"),
                        endTime = slotEndTime.ToString(@"hh\:mm")
                    });

                    currentTime = currentTime.Add(slotDuration);
                }

                return Json(new { success = true, timeSlots });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}