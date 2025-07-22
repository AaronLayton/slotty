using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Slotty.Configuration;
using Slotty.Exceptions;
using Slotty.Validation;

namespace Slotty.Services;

/// <summary>
/// Service for storing and retrieving slot content per HTTP request.
/// </summary>
public class SlotContentStore : ISlotContentStore
{
    private const string HttpContextKey = "Slotty_SlotContent";
    private const string DefinedSlotsKey = "Slotty_DefinedSlots";
    private const string FilledSlotsKey = "Slotty_FilledSlots";
    
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<SlotContentStore> _logger;
    private readonly SlottyOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SlotContentStore"/> class.
    /// </summary>
    /// <param name="httpContextAccessor">The HTTP context accessor.</param>
    /// <param name="logger">The logger for diagnostic messages.</param>
    /// <param name="options">The Slotty configuration options.</param>
    /// <exception cref="ArgumentNullException">Thrown when any parameter is null.</exception>
    public SlotContentStore(
        IHttpContextAccessor httpContextAccessor, 
        ILogger<SlotContentStore> logger,
        IOptions<SlottyOptions> options)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _options = options?.Value ?? throw new ArgumentNullException(nameof(options));
    }

    /// <summary>
    /// Gets the slot content dictionary for the current request.
    /// </summary>
    /// <returns>A dictionary of slot content keyed by slot name.</returns>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    public Dictionary<string, List<string>> GetSlotContent()
    {
        var httpContext = GetHttpContext();

        if (httpContext.Items[HttpContextKey] is not Dictionary<string, List<string>> content)
        {
            content = new Dictionary<string, List<string>>();
            httpContext.Items[HttpContextKey] = content;
        }
        return content;
    }

    /// <summary>
    /// Adds content to a named slot for the current request.
    /// </summary>
    /// <param name="slotName">The name of the slot.</param>
    /// <param name="content">The content to add.</param>
    /// <exception cref="ArgumentException">Thrown when slotName is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown when content is null.</exception>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    /// <exception cref="InvalidSlotNameException">Thrown when slot name is invalid.</exception>
    public void AddContent(string slotName, string content)
    {
        if (string.IsNullOrWhiteSpace(slotName))
            throw new ArgumentException("Slot name cannot be null or whitespace.", nameof(slotName));
        if (content is null)
            throw new ArgumentNullException(nameof(content));

        // Always validate slot name
        ValidateSlotName(slotName);

        var slotContent = GetSlotContent();
        
        if (!slotContent.ContainsKey(slotName))
        {
            slotContent[slotName] = new List<string>();
        }
        
        slotContent[slotName].Add(content);

        // Track filled slots only when validation mode is not Silent
        if (IsTrackingEnabled())
        {
            var filledSlots = GetFilledSlots();
            filledSlots.Add(slotName);
        }

        _logger.LogDebug("Added content to slot '{SlotName}' (length: {ContentLength})", slotName, content.Length);
    }

    /// <summary>
    /// Gets all content for a named slot.
    /// </summary>
    /// <param name="slotName">The name of the slot.</param>
    /// <returns>A list of content strings for the slot.</returns>
    /// <exception cref="ArgumentException">Thrown when slotName is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    public List<string> GetContent(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
            throw new ArgumentException("Slot name cannot be null or whitespace.", nameof(slotName));

        var slotContent = GetSlotContent();
        return slotContent.ContainsKey(slotName) ? slotContent[slotName] : new List<string>();
    }

    /// <summary>
    /// Registers that a slot has been defined in the current request.
    /// </summary>
    /// <param name="slotName">The name of the slot that was defined.</param>
    /// <exception cref="ArgumentException">Thrown when slotName is null or whitespace.</exception>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    /// <exception cref="InvalidSlotNameException">Thrown when slot name is invalid.</exception>
    public void RegisterSlotDefined(string slotName)
    {
        if (string.IsNullOrWhiteSpace(slotName))
            throw new ArgumentException("Slot name cannot be null or whitespace.", nameof(slotName));

        // Always validate slot name
        ValidateSlotName(slotName);

        if (IsTrackingEnabled())
        {
            var definedSlots = GetDefinedSlots();
            definedSlots.Add(slotName);
            _logger.LogDebug("Registered slot '{SlotName}' as defined", slotName);
        }
    }

    /// <summary>
    /// Gets the set of slot names that have been defined in the current request.
    /// </summary>
    /// <returns>A set of defined slot names.</returns>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    public HashSet<string> GetDefinedSlots()
    {
        var httpContext = GetHttpContext();

        if (httpContext.Items[DefinedSlotsKey] is not HashSet<string> definedSlots)
        {
            definedSlots = new HashSet<string>();
            httpContext.Items[DefinedSlotsKey] = definedSlots;
        }
        return definedSlots;
    }

    /// <summary>
    /// Gets the set of slot names that have been filled in the current request.
    /// </summary>
    /// <returns>A set of filled slot names.</returns>
    /// <exception cref="InvalidOperationException">Thrown when HttpContext is not available.</exception>
    public HashSet<string> GetFilledSlots()
    {
        var httpContext = GetHttpContext();

        if (httpContext.Items[FilledSlotsKey] is not HashSet<string> filledSlots)
        {
            filledSlots = new HashSet<string>();
            httpContext.Items[FilledSlotsKey] = filledSlots;
        }
        return filledSlots;
    }

    /// <summary>
    /// Validates slot usage and handles validation according to the configured mode.
    /// </summary>
    public void ValidateSlotUsage()
    {
        if (!IsTrackingEnabled())
            return;

        var definedSlots = GetDefinedSlots();
        var filledSlots = GetFilledSlots();
        var orphanedFills = filledSlots.Except(definedSlots).ToList();

        if (orphanedFills.Count == 0)
            return;

        var message = $"Found {orphanedFills.Count} orphaned fills (fills without corresponding slots): {string.Join(", ", orphanedFills.Select(s => $"'{s}'"))}";

        switch (_options.ValidationMode)
        {
            case SlottyValidationMode.Silent:
                // Should not reach here due to IsTrackingEnabled() check
                break;
            case SlottyValidationMode.Log:
                _logger.LogWarning("Slot validation warning: {Message}", message);
                break;
            case SlottyValidationMode.Throw:
                throw new SlotNotFoundException(orphanedFills.First(), message);
        }
    }

    private HttpContext GetHttpContext()
    {
        return _httpContextAccessor.HttpContext 
            ?? throw new InvalidOperationException("HttpContext is not available. This service can only be used within a web request.");
    }

    private void ValidateSlotName(string slotName)
    {
        if (!SlotNameValidator.IsValidSlotName(slotName))
        {
            throw new InvalidSlotNameException(slotName, 
                $"Invalid slot name '{slotName}'. Slot names must start with a letter and contain only letters, numbers, hyphens, and underscores. Optional modifiers :before and :after are supported.");
        }
    }

    private bool IsTrackingEnabled()
    {
        return _options.ValidationMode != SlottyValidationMode.Silent;
    }
} 