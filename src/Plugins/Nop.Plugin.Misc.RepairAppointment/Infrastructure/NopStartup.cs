using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.RepairAppointment.Services;

namespace Nop.Plugin.Misc.RepairAppointment.Infrastructure
{
    public class NopStartup : INopStartup
    {
        public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<IRepairAppointmentService, RepairAppointmentService>();
            services.AddScoped<IRepairCategoryService, RepairCategoryService>();
            services.AddScoped<IRepairProductService, RepairProductService>();
            services.AddScoped<IRepairTypeService, RepairTypeService>();
            services.AddScoped<ISlotCapacityService, SlotCapacityService>();
        }

        public void Configure(IApplicationBuilder application)
        {
        }

        public int Order => 11;
    }
}