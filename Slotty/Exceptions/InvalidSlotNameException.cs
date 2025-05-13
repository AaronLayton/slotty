namespace Slotty.Exceptions;

/// <summary>
/// Exception thrown when a slot name doesn't conform to naming conventions.
/// This exception is thrown when slot names contain invalid characters or don't follow expected patterns.
/// </summary>
public class InvalidSlotNameException : ArgumentException
{
    /// <summary>
    /// Gets the invalid slot name that caused the exception.
    /// </summary>
    public string SlotName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSlotNameException"/> class.
    /// </summary>
    /// <param name="slotName">The invalid slot name.</param>
    public InvalidSlotNameException(string slotName) 
        : base($"Invalid slot name '{slotName}'. Slot names must contain only letters, numbers, hyphens, underscores, and colons for extensions.")
    {
        SlotName = slotName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSlotNameException"/> class.
    /// </summary>
    /// <param name="slotName">The invalid slot name.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    public InvalidSlotNameException(string slotName, string message) : base(message)
    {
        SlotName = slotName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="InvalidSlotNameException"/> class.
    /// </summary>
    /// <param name="slotName">The invalid slot name.</param>
    /// <param name="message">The error message that explains the reason for the exception.</param>
    /// <param name="innerException">The exception that is the cause of the current exception.</param>
    public InvalidSlotNameException(string slotName, string message, Exception innerException) : base(message, innerException)
    {
        SlotName = slotName;
    }
} 