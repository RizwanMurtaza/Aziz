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

            model.AvailableDeviceTypes = new[]
            {
                new SelectListItem { Text = "Select Device Type", Value = "" },
                new SelectListItem { Text = "Mobile Phone", Value = "Mobile" },
                new SelectListItem { Text = "Laptop", Value = "Laptop" },
                new SelectListItem { Text = "Desktop", Value = "Desktop" },
                new SelectListItem { Text = "Tablet", Value = "Tablet" },
                new SelectListItem { Text = "Other", Value = "Other" }
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

            if (!ModelState.IsValid)
                return Json(new { success = false, message = "Please fill all required fields" });

            var isAvailable = await _appointmentService.IsSlotAvailableAsync(model.AppointmentDate, model.TimeSlotId);
            if (!isAvailable)
                return Json(new { success = false, message = "Selected time slot is no longer available" });

            var customer = await _workContext.GetCurrentCustomerAsync();
            var timeSlot = await _appointmentService.GetTimeSlotByIdAsync(model.TimeSlotId);

            var appointment = new Domain.RepairAppointment
            {
                CustomerName = model.CustomerName,
                Email = model.Email,
                Phone = model.Phone,
                DeviceType = model.DeviceType,
                DeviceBrand = model.DeviceBrand,
                DeviceModel = model.DeviceModel,
                IssueDescription = model.IssueDescription,
                AppointmentDate = model.AppointmentDate.Date.Add(timeSlot.StartTime),
                TimeSlot = timeSlot.TimeSlot,
                TimeSlotId = model.TimeSlotId,
                Status = AppointmentStatus.Confirmed,
                CustomerId = !await _customerService.IsGuestAsync(customer) ? customer.Id : null,
                ConfirmationSent = false,
                ReminderSent = false
            };

            await _appointmentService.InsertAppointmentAsync(appointment);

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

            var availableSlots = await _appointmentService.GetAvailableSlotsForDateAsync(date);

            var slots = availableSlots.Select(s => new
            {
                id = s.Id,
                text = s.TimeSlot,
                available = s.MaxAppointments - s.CurrentBookings
            }).ToList();

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
    }
}