using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Nop.Plugin.Misc.RepairAppointment;
using Nop.Services.Configuration;
using Nop.Web.Framework.Components;

namespace Nop.Plugin.Misc.RepairAppointment.Components
{
    public class RepairAppointmentBookingViewComponent : NopViewComponent
    {
        private readonly ISettingService _settingService;

        public RepairAppointmentBookingViewComponent(ISettingService settingService)
        {
            _settingService = settingService;
        }

        public async Task<IViewComponentResult> InvokeAsync(string widgetZone, object additionalData)
        {
            var settings = await _settingService.LoadSettingAsync<RepairAppointmentSettings>();

            if (!settings.EnableAppointmentSystem)
                return Content("");

            return View("~/Plugins/Misc.RepairAppointment/Views/Components/BookingLink.cshtml");
        }
    }
}