using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Nop.Core.Infrastructure;
using Nop.Plugin.Misc.SpireProductImport.Services;

namespace Nop.Plugin.Misc.SpireProductImport.Infrastructure;

/// <summary>
/// Represents a plugin startup configuration
/// </summary>
public class NopStartup : INopStartup
{
    /// <summary>
    /// Add and configure any of the middleware
    /// </summary>
    /// <param name="services">Collection of service descriptors</param>
    /// <param name="configuration">Configuration of the application</param>
    public void ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // Register FTP service
        services.AddScoped<IFtpService, FtpService>();

        // Register CSV parsing service
        services.AddScoped<ICsvParsingService, CsvParsingService>();

        // Register product import service
        services.AddScoped<IProductImportService, ProductImportService>();

        // Register scheduled task
        services.AddScoped<SpireProductImportTask>();
    }

    /// <summary>
    /// Configure the using of added middleware
    /// </summary>
    /// <param name="application">Builder for configuring an application's request pipeline</param>
    public void Configure(IApplicationBuilder application)
    {
        // No specific middleware configuration needed for this plugin
    }

    /// <summary>
    /// Gets order of this startup configuration implementation
    /// </summary>
    public int Order => 1000; // Should be run after Nop main startup
}