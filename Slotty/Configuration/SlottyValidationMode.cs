namespace Slotty.Configuration;

/// <summary>
/// Defines how Slotty should handle validation scenarios like missing slots or invalid content.
/// </summary>
public enum SlottyValidationMode
{
    /// <summary>
    /// Silent mode (default): No validation warnings or errors are generated.
    /// Missing slots are ignored, and invalid content is processed without notification.
    /// This maintains backward compatibility with existing implementations.
    /// </summary>
    Silent = 0,

    /// <summary>
    /// Log mode: Validation issues are logged using the configured logger, but processing continues.
    /// Useful for debugging and monitoring during development or production.
    /// </summary>
    Log = 1,

    /// <summary>
    /// Throw mode: Validation issues cause exceptions to be thrown.
    /// Use this mode when you want strict validation and want to catch issues early.
    /// </summary>
    Throw = 2
} 