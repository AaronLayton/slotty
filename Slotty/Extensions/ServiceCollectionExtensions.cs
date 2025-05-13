using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Slotty.Configuration;
using Slotty.Middleware;
using Slotty.Services;

namespace Slotty.Extensions;

/// <summary>
/// Extension methods for configuring Slotty services in the dependency injection container.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Slotty services to the dependency injection container with default configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// Configure Slotty with default settings:
    /// <code>
    /// services.AddSlotty();
    /// </code>
    /// </example>
    public static IServiceCollection AddSlotty(this IServiceCollection services)
    {
        return services.AddSlotty(_ => { });
    }

    /// <summary>
    /// Adds Slotty services to the dependency injection container with custom configuration.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configureOptions">Action to configure Slotty options.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// Configure Slotty with custom settings:
    /// <code>
    /// services.AddSlotty(options =>
    /// {
    ///     options.ValidationMode = SlottyValidationMode.Log;
    /// });
    /// </code>
    /// </example>
    public static IServiceCollection AddSlotty(this IServiceCollection services, Action<SlottyOptions> configureOptions)
    {
        services.Configure(configureOptions);
        RegisterServices(services);
        return services;
    }

    /// <summary>
    /// Adds Slotty services to the dependency injection container with configuration from a configuration section.
    /// </summary>
    /// <param name="services">The service collection to add services to.</param>
    /// <param name="configurationSection">The configuration section containing Slotty options.</param>
    /// <returns>The service collection for method chaining.</returns>
    /// <example>
    /// Configure Slotty from appsettings.json:
    /// <code>
    /// services.AddSlotty(configuration.GetSection("Slotty"));
    /// </code>
    /// </example>
    public static IServiceCollection AddSlotty(this IServiceCollection services, IConfigurationSection configurationSection)
    {
        services.Configure<SlottyOptions>(configurationSection);
        RegisterServices(services);
        return services;
    }

    private static void RegisterServices(IServiceCollection services)
    {
        services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
        services.TryAddScoped<ISlotContentStore, SlotContentStore>();
        services.TryAddScoped<ISlottyAssetService, SlottyAssetService>();
    }
}

/// <summary>
/// Extension methods for configuring Slotty middleware in the application pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds Slotty middleware to the application pipeline.
    /// This middleware automatically injects CSS and JavaScript for slot visualization
    /// based on the configured DevToolsInjection setting.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for method chaining.</returns>
    /// <example>
    /// Add Slotty middleware to the pipeline:
    /// <code>
    /// app.UseSlotty();
    /// </code>
    /// </example>
    public static IApplicationBuilder UseSlotty(this IApplicationBuilder app)
    {
        return app.UseMiddleware<SlottyDevToolsMiddleware>();
    }
} 