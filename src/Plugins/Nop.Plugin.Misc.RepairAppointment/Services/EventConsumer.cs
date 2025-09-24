using System.Collections.Generic;
using System.Threading.Tasks;
using Nop.Services.Plugins;
using Nop.Services.Security;
using Nop.Web.Framework.Events;
using Nop.Web.Framework.Menu;

namespace Nop.Plugin.Misc.RepairAppointment.Services
{
    public class EventConsumer : BaseAdminMenuCreatedEventConsumer
    {
        private readonly IPermissionService _permissionService;
        private readonly IAdminMenu _adminMenu;

        public EventConsumer(IPluginManager<IPlugin> pluginManager, IPermissionService permissionService, IAdminMenu adminMenu) :
            base(pluginManager)
        {
            _permissionService = permissionService;
            _adminMenu = adminMenu;
        }

        protected override string PluginSystemName => "Misc.RepairAppointment";

        protected override string BeforeMenuSystemName => "Configuration";

        protected override async Task<AdminMenuItem> GetAdminMenuItemAsync(IPlugin plugin)
        {
            if (!await _permissionService.AuthorizeAsync(StandardPermission.Configuration.MANAGE_PLUGINS))
                return null;

            var menuItem = new AdminMenuItem
            {
                Visible = true,
                SystemName = "RepairAppointments",
                Title = "Repair Appointments",
                IconClass = "fas fa-tools",
                ChildNodes = new List<AdminMenuItem>
                {
                    new()
                    {
                        Visible = true,
                        SystemName = "RepairAppointments.List",
                        Title = "View Appointments",
                        Url = _adminMenu.GetMenuItemUrl("RepairAppointment", "List"),
                        IconClass = "far fa-list-alt"
                    },
                    new()
                    {
                        Visible = true,
                        SystemName = "RepairAppointments.Categories",
                        Title = "Repair Categories",
                        Url = _adminMenu.GetMenuItemUrl("RepairCategory", "List"),
                        IconClass = "far fa-folder"
                    },
                    new()
                    {
                        Visible = true,
                        SystemName = "RepairAppointments.Products",
                        Title = "Repair Products",
                        Url = _adminMenu.GetMenuItemUrl("RepairProduct", "List"),
                        IconClass = "far fa-mobile-alt"
                    },
                    new()
                    {
                        Visible = true,
                        SystemName = "RepairAppointments.RepairTypes",
                        Title = "Repair Types",
                        Url = _adminMenu.GetMenuItemUrl("RepairType", "List"),
                        IconClass = "far fa-wrench"
                    },
                    new()
                    {
                        Visible = true,
                        SystemName = "RepairAppointments.SlotManagement",
                        Title = "Slot Management",
                        Url = _adminMenu.GetMenuItemUrl("SlotManagement", "List"),
                        IconClass = "far fa-calendar-times"
                    },
                    new()
                    {
                        Visible = true,
                        SystemName = "RepairAppointments.Settings",
                        Title = "Settings",
                        Url = plugin.GetConfigurationPageUrl(),
                        IconClass = "far fa-cog"
                    }
                }
            };

            return menuItem;
        }
    }
}