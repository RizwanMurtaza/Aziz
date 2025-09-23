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
                ["Plugins.Misc.RepairAppointment.Search.StatusId"] = "Status",
                ["Plugins.Misc.RepairAppointment.Search.SearchText"] = "Search Text",

                // Admin messages
                ["Admin.Plugins.Misc.RepairAppointment.List"] = "Repair Appointments",
                ["Admin.Plugins.Misc.RepairAppointment.Created"] = "The appointment has been created successfully",
                ["Admin.Plugins.Misc.RepairAppointment.Updated"] = "The appointment has been updated successfully",
                ["Admin.Plugins.Misc.RepairAppointment.Deleted"] = "The appointment has been deleted successfully",

                // RepairCategory fields
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.Name"] = "Name",
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.Description"] = "Description",
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.IsActive"] = "Active",
                ["Admin.Plugins.Misc.RepairAppointment.RepairCategories.Fields.DisplayOrder"] = "Display Order",

                // RepairProduct fields
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Name"] = "Name",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Category"] = "Category",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Brand"] = "Brand",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.Model"] = "Model",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.IsActive"] = "Active",
                ["Admin.Plugins.Misc.RepairAppointment.RepairProducts.Fields.DisplayOrder"] = "Display Order",

                // RepairType fields
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.Name"] = "Name",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.Category"] = "Category",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.Product"] = "Product",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.EstimatedPrice"] = "Estimated Price",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.EstimatedDuration"] = "Duration (minutes)",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.IsActive"] = "Active",
                ["Admin.Plugins.Misc.RepairAppointment.RepairTypes.Fields.DisplayOrder"] = "Display Order",

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