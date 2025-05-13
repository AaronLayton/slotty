namespace Slotty.Configuration;

/// <summary>
/// Defines when Slotty development tools (CSS and JavaScript for slot visualization) should be injected.
/// </summary>
public enum SlottyDevToolsInjectionMode
{
    /// <summary>
    /// Automatically inject dev tools based on environment (Development only).
    /// This is the default and recommended setting for most applications.
    /// </summary>
    Auto = 0,

    /// <summary>
    /// Always inject dev tools regardless of environment.
    /// Use this when you need slot visualization in staging or production environments.
    /// </summary>
    Always = 1,

    /// <summary>
    /// Never inject dev tools.
    /// Use this to completely disable slot visualization features.
    /// </summary>
    Never = 2
} 