using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
using Nop.Web.Framework.Mvc.Routing;

namespace Nop.Plugin.Misc.RepairAppointment.Infrastructure
{
    public partial class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(IEndpointRouteBuilder endpointRouteBuilder)
        {
            endpointRouteBuilder.MapControllerRoute(
                "Plugin.Misc.RepairAppointment.BookAppointment",
                "repair-appointment/book",
                new { controller = "RepairAppointmentPublic", action = "BookAppointment" });

            endpointRouteBuilder.MapControllerRoute(
                "Plugin.Misc.RepairAppointment.GetAvailableSlots",
                "repair-appointment/available-slots",
                new { controller = "RepairAppointmentPublic", action = "GetAvailableSlots" });

            endpointRouteBuilder.MapControllerRoute(
                "Plugin.Misc.RepairAppointment.AppointmentConfirmation",
                "repair-appointment/confirmation/{confirmationCode}",
                new { controller = "RepairAppointmentPublic", action = "AppointmentConfirmation" });
        }

        public int Priority => 0;
    }
}