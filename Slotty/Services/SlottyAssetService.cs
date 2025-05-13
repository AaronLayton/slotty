using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Slotty.Configuration;
using System.Reflection;

namespace Slotty.Services;

/// <summary>
/// Service for determining when Slotty development assets should be injected and providing asset content.
/// </summary>
public class SlottyAssetService : ISlottyAssetService
{
    private readonly SlottyOptions _options;
    private readonly IWebHostEnvironment _environment;
    private string? _cssContent;
    private string? _jsContent;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlottyAssetService"/> class.
    /// </summary>
    /// <param name="options">The Slotty configuration options.</param>
    /// <param name="environment">The web host environment.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SlottyAssetService(IOptions<SlottyOptions> options, IWebHostEnvironment environment)
    {
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
        _environment = environment ?? throw new ArgumentNullException(nameof(environment));
    }

    /// <summary>
    /// Determines whether Slotty development tools should be injected based on configuration and environment.
    /// </summary>
    /// <returns>True if dev tools should be injected, false otherwise.</returns>
    public bool ShouldInjectAssets()
    {
        return _options.DevToolsInjection switch
        {
            SlottyDevToolsInjectionMode.Always => true,
            SlottyDevToolsInjectionMode.Never => false,
            SlottyDevToolsInjectionMode.Auto => _environment.IsDevelopment(),
            _ => false
        };
    }

    /// <summary>
    /// Gets the CSS content for Slotty development mode.
    /// </summary>
    /// <returns>The CSS content as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when CSS content cannot be loaded.</exception>
    public string GetCssContent()
    {
        if (_cssContent is not null)
            return _cssContent;

        _cssContent = LoadEmbeddedResource("Slotty.wwwroot.css.slotty.css");
        return _cssContent;
    }

    /// <summary>
    /// Gets the JavaScript content for Slotty development mode.
    /// </summary>
    /// <returns>The JavaScript content as a string.</returns>
    /// <exception cref="InvalidOperationException">Thrown when JavaScript content cannot be loaded.</exception>
    public string GetJavaScriptContent()
    {
        if (_jsContent is not null)
            return _jsContent;

        _jsContent = LoadEmbeddedResource("Slotty.wwwroot.js.slotty.js");
        return _jsContent;
    }

    private string LoadEmbeddedResource(string resourceName)
    {
        var assembly = Assembly.GetExecutingAssembly();
        
        using var stream = assembly.GetManifestResourceStream(resourceName);
        if (stream is null)
            throw new InvalidOperationException($"Embedded resource '{resourceName}' not found.");

        using var reader = new StreamReader(stream);
        return reader.ReadToEnd();
    }
} 