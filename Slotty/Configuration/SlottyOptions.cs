namespace Slotty.Configuration;

/// <summary>
/// Configuration options for the Slotty package.
/// </summary>
public class SlottyOptions
{
    /// <summary>
    /// Gets or sets the validation mode for slot operations.
    /// Determines how validation errors are handled for orphaned fills.
    /// Slot name validation is always performed regardless of this setting.
    /// </summary>
    public SlottyValidationMode ValidationMode { get; set; } = SlottyValidationMode.Silent;

    /// <summary>
    /// Gets or sets when to inject development tools (CSS and JavaScript for slot visualization).
    /// Auto: Inject in Development environment only (default)
    /// Always: Always inject dev tools regardless of environment  
    /// Never: Never inject dev tools
    /// </summary>
    public SlottyDevToolsInjectionMode DevToolsInjection { get; set; } = SlottyDevToolsInjectionMode.Auto;
} 