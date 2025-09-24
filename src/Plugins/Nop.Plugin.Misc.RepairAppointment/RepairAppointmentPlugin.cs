using Nop.Core;
using Nop.Core.Domain.Cms;
using Nop.Core.Domain.Messages;
using Nop.Data;
using Nop.Plugin.Misc.RepairAppointment.Domain;
using Nop.Services.Cms;
using Nop.Services.Configuration;
using Nop.Services.Localization;
using Nop.Services.Plugins;
using Nop.Web.Framework.Infrastructure;
using Nop.Web.Framework.Menu;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Nop.Plugin.Misc.RepairAppointment
{
    public class RepairAppointmentPlugin : BasePlugin, IWidgetPlugin
    {
        private readonly ILocalizationService _localizationService;
        private readonly IWebHelper _webHelper;
        private readonly ISettingService _settingService;
        private readonly WidgetSettings _widgetSettings;
        private readonly IRepository<RepairCategory> _repairCategoryRepository;

        public RepairAppointmentPlugin(
            ILocalizationService localizationService,
            IWebHelper webHelper,
            ISettingService settingService,
            WidgetSettings widgetSettings,
            IRepository<RepairCategory> repairCategoryRepository)
        {
            _localizationService = localizationService;
            _webHelper = webHelper;
            _settingService = settingService;
            _widgetSettings = widgetSettings;
            _repairCategoryRepository = repairCategoryRepository;
        }

        public bool HideInWidgetList => false;

        public override string GetConfigurationPageUrl()
        {
            return $"{_webHelper.GetStoreLocation()}Admin/RepairAppointment/Configure";
        }


        public override async Task InstallAsync()
        {
            var settings = new RepairAppointmentSettings
            {
                EnableAppointmentSystem = true,
                SlotDurationMinutes = 30,
                MaxSlotsPerDay = 16,
                BusinessStartTime = "09:00",
                BusinessEndTime = "17:00",
                SendConfirmationEmail = true,
                SendReminderEmail = true,
                ReminderHoursBeforeAppointment = 24
            };

            await _settingService.SaveSettingAsync(settings);

            // Create default repair categories
            await CreateDefaultRepairCategoriesAsync();

            await _localizationService.AddOrUpdateLocaleResourceAsync(new Dictionary<string, string>
            {
                // Main fields
                ["Plugins.Misc.RepairAppointment.Fields.CustomerName"] = "Customer Name",
                ["Plugins.Misc.RepairAppointment.Fields.Email"] = "Email",
                ["Plugins.Misc.RepairAppointment.Fields.Phone"] = "Phone Number",
                ["Plugins.Misc.RepairAppointment.Fields.DeviceType"] = "Device Type",
                ["Plugins.Misc.RepairAppointment.Fields.DeviceBrand"] = "Device Brand",
                ["Plugins.Misc.RepairAppointment.Fields.DeviceModel"] = "Device Model",
                ["Plugins.Misc.RepairAppointment.Fields.IssueDescription"] = "Issue Description",
                ["Plugins.Misc.RepairAppointment.Fields.AppointmentDate"] = "Appointment Date",
                ["Plugins.Misc.RepairAppointment.Fields.TimeSlot"] = "Time Slot",
                ["Plugins.Misc.RepairAppointment.Fields.Status"] = "Status",
                ["Plugins.Misc.RepairAppointment.Fields.RepairCategory"] = "Repair Category",
                ["Plugins.Misc.RepairAppointment.Fields.RepairProduct"] = "Product/Device",
                ["Plugins.Misc.RepairAppointment.Fields.RepairType"] = "Repair Type",
                ["Plugins.Misc.RepairAppointment.Fields.EstimatedPrice"] = "Estimated Price",
                ["Plugins.Misc.RepairAppointment.Fields.ConfirmationCode"] = "Confirmation Code",
                ["Plugins.Misc.RepairAppointment.Fields.CreatedOn"] = "Created On",

                // Search fields
                ["Plugins.Misc.RepairAppointment.Search.FromDate"] = "From Date",
                ["Plugins.Misc.RepairAppointment.Search.ToDate"] = "To Date",
                ["Plugins.Misc.RepairAppointment.Search.Status"] = "Status",
                ["Plugins.Misc.RepairAppointment.Search.StatusId"] = "Status",
                ["Plugins.Misc.RepairAppointment.Search.SearchText"] = "Search Text",

                // Admin messages
                ["Admin.Plugins.Misc.RepairAppointment.List"] = "Repair Appointments",
                ["Admin.Plugins.Misc.RepairAppointment.Created"] = "The appointment has been created successfully",
                ["Admin.Plugins.Misc.RepairAppointment.Updated"] = "The appointment has been updated successfully",
                ["Admin.Plugins.Misc.RepairAppointment.Deleted"] = "The appointment has been deleted successfully",

                // RepairCategory fields (Form)
                ["Plugins.Misc.RepairAppointment.RepairCategory.Fields.Name"] = "Name",
                ["Plugins.Misc.RepairAppointment.RepairCategory.Fields.Description"] = "Description",
                ["Plugins.Misc.RepairAppointment.RepairCategory.Fields.IsActive"] = "Is Active",
                ["Plugins.Misc.RepairAppointment.RepairCategory.Fields.DisplayOrder"] = "Display Order",
                ["Plugins.Misc.RepairAppointment.RepairCategory.Search.Name"] = "Search by Name",
                ["Plugins.Misc.RepairAppointment.RepairCategory.Search.IsActive"] = "Is Active",

                // RepairCategory fields (Grid)
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.Name"] = "Name",
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.Description"] = "Description",
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.IsActive"] = "Active",
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.DisplayOrder"] = "Display Order",

                // RepairProduct fields (Form)
                ["Plugins.Misc.RepairAppointment.RepairProduct.Fields.RepairCategoryId"] = "Category",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Fields.Name"] = "Name",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Fields.Brand"] = "Brand",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Fields.Model"] = "Model",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Fields.Description"] = "Description",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Fields.IsActive"] = "Is Active",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Fields.DisplayOrder"] = "Display Order",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Search.CategoryId"] = "Category",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Search.Name"] = "Search by Name",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Search.Brand"] = "Search by Brand",
                ["Plugins.Misc.RepairAppointment.RepairProduct.Search.IsActive"] = "Is Active",

                // RepairProduct fields (Grid)
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Name"] = "Name",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Category"] = "Category",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Brand"] = "Brand",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Model"] = "Model",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.IsActive"] = "Active",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.DisplayOrder"] = "Display Order",

                // RepairType fields (Form)
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.RepairCategoryId"] = "Category",
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.RepairProductId"] = "Product (Optional)",
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.Name"] = "Name",
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.Description"] = "Description",
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.EstimatedPrice"] = "Estimated Price",
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.EstimatedDurationMinutes"] = "Estimated Duration (Minutes)",
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.IsActive"] = "Is Active",
                ["Plugins.Misc.RepairAppointment.RepairType.Fields.DisplayOrder"] = "Display Order",
                ["Plugins.Misc.RepairAppointment.RepairType.Search.CategoryId"] = "Category",
                ["Plugins.Misc.RepairAppointment.RepairType.Search.ProductId"] = "Product",
                ["Plugins.Misc.RepairAppointment.RepairType.Search.Name"] = "Search by Name",
                ["Plugins.Misc.RepairAppointment.RepairType.Search.IsActive"] = "Is Active",

                // RepairType fields (Grid)
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.Name"] = "Name",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.Category"] = "Category",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.Product"] = "Product",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.EstimatedPrice"] = "Price",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.EstimatedDurationMinutes"] = "Duration (Min)",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.IsActive"] = "Active",

                // Configuration page
                ["Plugins.Misc.RepairAppointment.Configuration.EnableAppointmentSystem"] = "Enable Appointment System",
                ["Plugins.Misc.RepairAppointment.Configuration.SlotDurationMinutes"] = "Slot Duration (Minutes)",
                ["Plugins.Misc.RepairAppointment.Configuration.MaxSlotsPerDay"] = "Max Slots Per Day",
                ["Plugins.Misc.RepairAppointment.Configuration.BusinessStartTime"] = "Business Start Time",
                ["Plugins.Misc.RepairAppointment.Configuration.BusinessEndTime"] = "Business End Time",
                ["Plugins.Misc.RepairAppointment.Configuration.SendConfirmationEmail"] = "Send Confirmation Email",
                ["Plugins.Misc.RepairAppointment.Configuration.SendReminderEmail"] = "Send Reminder Email",
                ["Plugins.Misc.RepairAppointment.Configuration.ReminderHoursBeforeAppointment"] = "Reminder Hours Before Appointment",
                ["Plugins.Misc.RepairAppointment.Configuration.MaxAdvanceBookingDays"] = "Max Advance Booking Days",
                ["Plugins.Misc.RepairAppointment.Configuration.RequireCustomerLogin"] = "Require Customer Login",

                // Common dropdown options
                ["Admin.Common.All"] = "All",
                ["Admin.Common.Active"] = "Active",
                ["Admin.Common.Inactive"] = "Inactive",
                ["Admin.Common.None"] = "None",
                ["Admin.Common.Select"] = "Select...",

                // Public messages
                ["Plugins.Misc.RepairAppointment.BookAppointment"] = "Book Repair Appointment",
                ["Plugins.Misc.RepairAppointment.AvailableSlots"] = "Available Time Slots",
                ["Plugins.Misc.RepairAppointment.NoSlotsAvailable"] = "No slots available for selected date",
                ["Plugins.Misc.RepairAppointment.AppointmentConfirmed"] = "Your appointment has been confirmed",
                ["Plugins.Misc.RepairAppointment.ConfirmationEmailSubject"] = "Repair Appointment Confirmation",
                ["Plugins.Misc.RepairAppointment.ReminderEmailSubject"] = "Repair Appointment Reminder"
            });

            if (!_widgetSettings.ActiveWidgetSystemNames.Contains("Misc.RepairAppointment"))
            {
                _widgetSettings.ActiveWidgetSystemNames.Add("Misc.RepairAppointment");
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await base.InstallAsync();
        }

        public override async Task UninstallAsync()
        {
            await _settingService.DeleteSettingAsync<RepairAppointmentSettings>();

            await _localizationService.DeleteLocaleResourcesAsync("Plugins.Misc.RepairAppointment");

            if (_widgetSettings.ActiveWidgetSystemNames.Contains("Misc.RepairAppointment"))
            {
                _widgetSettings.ActiveWidgetSystemNames.Remove("Misc.RepairAppointment");
                await _settingService.SaveSettingAsync(_widgetSettings);
            }

            await base.UninstallAsync();
        }

        public Task<IList<string>> GetWidgetZonesAsync()
        {
            return Task.FromResult<IList<string>>(new List<string>
            {
                PublicWidgetZones.HeaderMenuBefore,
                PublicWidgetZones.HeaderMenuAfter
            });
        }

        public Type GetWidgetViewComponent(string widgetZone)
        {
            return typeof(Components.RepairAppointmentBookingViewComponent);
        }

        private async Task CreateDefaultRepairCategoriesAsync()
        {
            var defaultCategories = new[]
            {
                new { Name = "Mobile Repair", Description = "Smartphone and mobile device repairs", DisplayOrder = 1 },
                new { Name = "PC Repair", Description = "Desktop computer and PC repairs", DisplayOrder = 2 },
                new { Name = "Laptop Repair", Description = "Laptop and notebook computer repairs", DisplayOrder = 3 },
                new { Name = "Tablet Repair", Description = "Tablet and iPad repairs", DisplayOrder = 4 },
                new { Name = "Other", Description = "Other electronic device repairs", DisplayOrder = 5 }
            };

            foreach (var categoryData in defaultCategories)
            {
                var existingCategory = await _repairCategoryRepository.Table
                    .Where(c => c.Name == categoryData.Name)
                    .FirstOrDefaultAsync();

                if (existingCategory == null)
                {
                    var category = new RepairCategory
                    {
                        Name = categoryData.Name,
                        Description = categoryData.Description,
                        IsActive = true,
                        DisplayOrder = categoryData.DisplayOrder,
                        CreatedOnUtc = DateTime.UtcNow
                    };

                    await _repairCategoryRepository.InsertAsync(category);
                }
            }
        }
    }
}