using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nop.Core;
using Nop.Core.Domain.Customers;
using Nop.Core.Domain.Messages;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Plugin.Misc.RepairAppointment.Models;
using Nop.Plugin.Misc.RepairAppointment.Services;
using Nop.Services.Configuration;
using Nop.Services.Customers;
using Nop.Services.Localization;
using Nop.Services.Messages;
using Nop.Web.Framework.Controllers;

namespace Nop.Plugin.Misc.RepairAppointment.Controllers
{
    public class RepairAppointmentPublicController : BasePluginController
    {
        private readonly IRepairAppointmentService _appointmentService;
        private readonly IRepairCategoryService _repairCategoryService;
        private readonly IRepairProductService _repairProductService;
        private readonly IRepairTypeService _repairTypeService;
        private readonly ISlotCapacityService _slotCapacityService;
        private readonly ILocalizationService _localizationService;
        private readonly INotificationService _notificationService;
        private readonly ISettingService _settingService;
        private readonly IStoreContext _storeContext;
        private readonly IWorkContext _workContext;
        private readonly ICustomerService _customerService;
        private readonly IEmailAccountService _emailAccountService;
        private readonly IQueuedEmailService _queuedEmailService;
        private readonly EmailAccountSettings _emailAccountSettings;

        public RepairAppointmentPublicController(
            IRepairAppointmentService appointmentService,
            IRepairCategoryService repairCategoryService,
            IRepairProductService repairProductService,
            IRepairTypeService repairTypeService,
            ISlotCapacityService slotCapacityService,
            ILocalizationService localizationService,
            INotificationService notificationService,
            ISettingService settingService,
            IStoreContext storeContext,
            IWorkContext workContext,
            ICustomerService customerService,
            IEmailAccountService emailAccountService,
            IQueuedEmailService queuedEmailService,
            EmailAccountSettings emailAccountSettings)
        {
            _appointmentService = appointmentService;
            _repairCategoryService = repairCategoryService;
            _repairProductService = repairProductService;
            _repairTypeService = repairTypeService;
            _slotCapacityService = slotCapacityService;
            _localizationService = localizationService;
            _notificationService = notificationService;
            _settingService = settingService;
            _storeContext = storeContext;
            _workContext = workContext;
            _customerService = customerService;
            _emailAccountService = emailAccountService;
            _queuedEmailService = queuedEmailService;
            _emailAccountSettings = emailAccountSettings;
        }

        public async Task<IActionResult> BookAppointment()
        {
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();

            if (!settings.EnableAppointmentSystem)
                return Content("Appointment system is currently disabled");

            var customer = await _workContext.GetCurrentCustomerAsync();
            if (settings.RequireCustomerLogin && await _customerService.IsGuestAsync(customer))
            {
                return Challenge();
            }

            var model = new RepairAppointmentModel();

            if (!await _customerService.IsGuestAsync(customer))
            {
                model.CustomerName = $"{customer.FirstName} {customer.LastName}".Trim();
                model.Email = customer.Email;
                model.Phone = customer.Phone;
            }

            // Load categories for Device Type dropdown
            var categories = await _repairCategoryService.GetAllRepairCategoriesAsync(isActive: true);
            model.AvailableDeviceTypes = categories.Select(c => new SelectListItem
            {
                Text = c.Name,
                Value = c.Id.ToString()
            }).ToList();
            model.AvailableDeviceTypes.Insert(0, new SelectListItem { Text = "Select Device Type", Value = "" });

            // Initialize empty dropdowns that will be populated via AJAX
            model.AvailableProducts = new List<SelectListItem>
            {
                new SelectListItem { Text = "Select device type first", Value = "" }
            };

            model.AvailableRepairTypes = new List<SelectListItem>
            {
                new SelectListItem { Text = "Select product first", Value = "" }
            };

            model.AppointmentDate = DateTime.Today.AddDays(1);

            return View("~/Plugins/Misc.RepairAppointment/Views/Public/BookAppointment.cshtml", model);
        }

        [HttpPost]
        public async Task<IActionResult> BookAppointment(RepairAppointmentModel model)
        {
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();

            if (!settings.EnableAppointmentSystem)
                return Json(new { success = false, message = "Appointment system is currently disabled" });

            // Handle "Other" selections first before validation
            if (string.IsNullOrEmpty(model.DeviceType) && model.RepairCategoryId.HasValue)
            {
                var category = await _repairCategoryService.GetRepairCategoryByIdAsync(model.RepairCategoryId.Value);
                if (category != null)
                    model.DeviceType = category.Name;
                else
                    model.DeviceType = "Other";
            }
            else if (string.IsNullOrEmpty(model.DeviceType))
            {
                model.DeviceType = "Other";
            }

            // Handle product "Other" selection
            if (model.RepairProductId.HasValue && model.RepairProductId == 0)
            {
                model.DeviceBrand = model.DeviceBrand ?? "Other";
                model.DeviceModel = model.DeviceModel ?? "Other";
            }

            // Handle repair type "Other" selection
            if (model.RepairTypeId.HasValue && model.RepairTypeId == 0)
            {
                // Allow "Other" repair type
                model.RepairTypeId = null;
            }

            if (!ModelState.IsValid)
            {
                // Create detailed validation error message
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                var errorMessage = errors.Any() ? string.Join(", ", errors) : "Please fill all required fields";
                return Json(new { success = false, message = errorMessage });
            }


            // Parse the TimeSlotId to extract date and time information
            if (!ParseTimeSlotId(model.TimeSlotId, out var slotDate, out var startTime, out var endTime))
                return Json(new { success = false, message = "Invalid time slot format" });

            // Validate the parsed date matches the selected appointment date
            if (slotDate.Date != model.AppointmentDate.Date)
                return Json(new { success = false, message = "Time slot date does not match appointment date" });

            // Check slot availability using the new slot capacity system
            var (maxCapacity, currentBookings) = await _slotCapacityService.GetEffectiveSlotCapacityAsync(model.AppointmentDate, startTime, endTime);
            var availableCapacity = maxCapacity - currentBookings;

            if (availableCapacity <= 0)
                return Json(new { success = false, message = "Selected time slot is no longer available" });

            // Validate the appointment date
            if (model.AppointmentDate < DateTime.Today)
                return Json(new { success = false, message = "Cannot book appointments for past dates" });

            var customer = await _workContext.GetCurrentCustomerAsync();

            // Create appointment date by combining the selected date with the time slot start time
            var appointmentDateTime = new DateTime(
                model.AppointmentDate.Year,
                model.AppointmentDate.Month,
                model.AppointmentDate.Day,
                startTime.Hours,
                startTime.Minutes,
                startTime.Seconds
            );

            var appointment = new Domain.RepairAppointment
            {
                CustomerName = model.CustomerName,
                Email = model.Email,
                Phone = model.Phone,
                DeviceType = model.DeviceType,
                DeviceBrand = model.DeviceBrand,
                DeviceModel = model.DeviceModel,
                IssueDescription = model.IssueDescription,
                AppointmentDate = appointmentDateTime,
                TimeSlot = $"{startTime:hh\\:mm} - {endTime:hh\\:mm}",
                TimeSlotId = model.TimeSlotId,
                Status = AppointmentStatus.Confirmed,
                CustomerId = !await _customerService.IsGuestAsync(customer) ? customer.Id : null,
                ConfirmationSent = false,
                ReminderSent = false,
                CreatedOnUtc = DateTime.UtcNow,
                ConfirmationCode = Guid.NewGuid().ToString("N")[..8].ToUpper()
            };

            await _appointmentService.InsertAppointmentAsync(appointment);

            // Update slot booking count
            await _slotCapacityService.UpdateSlotBookingCountAsync(
                appointment.AppointmentDate.Date,
                startTime,
                endTime,
                increment: true);

            if (settings.SendConfirmationEmail)
            {
                await SendConfirmationEmailAsync(appointment);
            }

            return Json(new
            {
                success = true,
                message = "Your appointment has been booked successfully!",
                confirmationCode = appointment.ConfirmationCode,
                redirectUrl = Url.Action("AppointmentConfirmation", new { confirmationCode = appointment.ConfirmationCode })
            });
        }

        public async Task<IActionResult> GetAvailableSlots(DateTime date)
        {
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();

            if (date < DateTime.Today)
                return Json(new { slots = new object[0] });

            if (settings.MaxAdvanceBookingDays > 0 && date > DateTime.Today.AddDays(settings.MaxAdvanceBookingDays))
                return Json(new { slots = new object[0] });

            // Check if the selected date is a working day
            if (!string.IsNullOrEmpty(settings.WorkingDays))
            {
                var workingDays = settings.WorkingDays.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(int.Parse)
                                                     .ToList();
                var dayOfWeek = (int)date.DayOfWeek;

                if (!workingDays.Contains(dayOfWeek))
                    return Json(new { slots = new object[0] });
            }

            // Generate time slots based on business hours
            var slots = new List<object>();

            if (!TimeSpan.TryParse(settings.BusinessStartTime, out var startTime) ||
                !TimeSpan.TryParse(settings.BusinessEndTime, out var endTime))
            {
                return Json(new { slots = new object[0] });
            }

            var slotDuration = TimeSpan.FromMinutes(settings.SlotDurationMinutes);
            var currentTime = startTime;

            while (currentTime.Add(slotDuration) <= endTime)
            {
                var slotEndTime = currentTime.Add(slotDuration);

                // Get effective capacity for this slot (considers both default settings and overrides)
                var (maxCapacity, currentBookings) = await _slotCapacityService.GetEffectiveSlotCapacityAsync(date, currentTime, slotEndTime);

                // Only include slot if it has available capacity and is active
                var availableCapacity = maxCapacity - currentBookings;
                if (availableCapacity > 0)
                {
                    slots.Add(new
                    {
                        id = $"{date:yyyy-MM-dd}_{currentTime:hh\\:mm}_{slotEndTime:hh\\:mm}",
                        text = $"{currentTime:hh\\:mm} - {slotEndTime:hh\\:mm}",
                        available = availableCapacity,
                        total = maxCapacity
                    });
                }

                currentTime = currentTime.Add(slotDuration);
            }

            return Json(new { slots });
        }

        public async Task<IActionResult> AppointmentConfirmation(string confirmationCode)
        {
            if (string.IsNullOrEmpty(confirmationCode))
                return RedirectToAction("BookAppointment");

            var appointment = await _appointmentService.GetAppointmentByConfirmationCodeAsync(confirmationCode);
            if (appointment == null)
                return RedirectToAction("BookAppointment");

            var model = new RepairAppointmentModel
            {
                CustomerName = appointment.CustomerName,
                Email = appointment.Email,
                Phone = appointment.Phone,
                DeviceType = appointment.DeviceType,
                DeviceBrand = appointment.DeviceBrand,
                DeviceModel = appointment.DeviceModel,
                IssueDescription = appointment.IssueDescription,
                AppointmentDate = appointment.AppointmentDate,
                TimeSlot = appointment.TimeSlot,
                ConfirmationCode = appointment.ConfirmationCode,
                StatusName = appointment.Status.ToString()
            };

            return View("~/Plugins/Misc.RepairAppointment/Views/Public/AppointmentConfirmation.cshtml", model);
        }

        private async Task SendConfirmationEmailAsync(Domain.RepairAppointment appointment)
        {
            var emailAccount = await _emailAccountService.GetEmailAccountByIdAsync(_emailAccountSettings.DefaultEmailAccountId);
            if (emailAccount == null)
                emailAccount = (await _emailAccountService.GetAllEmailAccountsAsync()).FirstOrDefault();

            if (emailAccount == null)
                return;

            var subject = await _localizationService.GetResourceAsync("Plugins.Misc.RepairAppointment.ConfirmationEmailSubject");

            var body = $@"
                <h2>Repair Appointment Confirmation</h2>
                <p>Dear {appointment.CustomerName},</p>
                <p>Your repair appointment has been confirmed with the following details:</p>
                <ul>
                    <li><strong>Confirmation Code:</strong> {appointment.ConfirmationCode}</li>
                    <li><strong>Date:</strong> {appointment.AppointmentDate:dddd, MMMM dd, yyyy}</li>
                    <li><strong>Time:</strong> {appointment.TimeSlot}</li>
                    <li><strong>Device:</strong> {appointment.DeviceType} - {appointment.DeviceBrand} {appointment.DeviceModel}</li>
                    <li><strong>Issue:</strong> {appointment.IssueDescription}</li>
                </ul>
                <p>Please arrive 5-10 minutes before your appointment time.</p>
                <p>If you need to cancel or reschedule, please contact us as soon as possible.</p>
                <p>Thank you for choosing our repair services!</p>
            ";

            await _queuedEmailService.InsertQueuedEmailAsync(new Core.Domain.Messages.QueuedEmail
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

            appointment.ConfirmationSent = true;
            await _appointmentService.UpdateAppointmentAsync(appointment);
        }

        // Auto-complete API endpoints
        public async Task<IActionResult> SearchCategories(string term)
        {
            var categories = await _repairCategoryService.SearchRepairCategoriesAsync(term, 10);
            var results = categories.Select(c => new
            {
                id = c.Id,
                text = c.Name,
                description = c.Description
            }).ToList();

            return Json(results);
        }

        public async Task<IActionResult> SearchProducts(string term, int? categoryId = null)
        {
            var products = await _repairProductService.SearchRepairProductsAsync(categoryId, term, 10);
            var results = products.Select(p => new
            {
                id = p.Id,
                text = $"{p.Brand} {p.Model} - {p.Name}".Trim(),
                brand = p.Brand,
                model = p.Model,
                name = p.Name,
                categoryId = p.RepairCategoryId
            }).ToList();

            return Json(results);
        }

        public async Task<IActionResult> SearchRepairTypes(string term, int? categoryId = null, int? productId = null)
        {
            var repairTypes = await _repairTypeService.SearchRepairTypesAsync(categoryId, productId, term, 10);
            var results = repairTypes.Select(rt => new
            {
                id = rt.Id,
                text = rt.Name,
                description = rt.Description,
                price = rt.EstimatedPrice,
                duration = rt.EstimatedDurationMinutes,
                categoryId = rt.RepairCategoryId,
                productId = rt.RepairProductId
            }).ToList();

            return Json(results);
        }

        public async Task<IActionResult> GetProductsByCategory(int categoryId)
        {
            var products = await _repairProductService.GetRepairProductsByCategoryIdAsync(categoryId);
            var results = products.Select(p => new
            {
                id = p.Id,
                text = $"{p.Brand} {p.Model} - {p.Name}".Trim(),
                brand = p.Brand,
                model = p.Model,
                name = p.Name
            }).ToList();

            return Json(results);
        }

        public async Task<IActionResult> GetRepairTypesByProduct(int productId)
        {
            var repairTypes = await _repairTypeService.GetRepairTypesByProductIdAsync(productId);
            var results = repairTypes.Select(rt => new
            {
                id = rt.Id,
                text = rt.Name,
                description = rt.Description,
                price = rt.EstimatedPrice,
                duration = rt.EstimatedDurationMinutes
            }).ToList();

            return Json(results);
        }

        public async Task<IActionResult> GetRepairTypeDetails(int repairTypeId)
        {
            var repairType = await _repairTypeService.GetRepairTypeByIdAsync(repairTypeId);
            if (repairType == null)
                return Json(null);

            return Json(new
            {
                id = repairType.Id,
                name = repairType.Name,
                description = repairType.Description,
                estimatedPrice = repairType.EstimatedPrice,
                estimatedDurationMinutes = repairType.EstimatedDurationMinutes
            });
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