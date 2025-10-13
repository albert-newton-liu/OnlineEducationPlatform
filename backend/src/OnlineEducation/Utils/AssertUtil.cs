namespace OnlineEducation.Utils;

/// <summary>
/// Utility class for assertions.
/// </summary>
public class AssertUtil
{
    /// <summary>
    /// Asserts that the given object is not null.
    /// </summary>
    public static void AssertNotNull(object? obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
    }

    /// <summary>
    /// Asserts that at least one of the given objects is not null.
    /// </summary>
    public static void AssertBothNotNull(object? obj1, object? ob2)
    {
        if (obj1 == null && ob2 == null)
        {
            throw new ArgumentException($"{nameof(obj1)} and {nameof(ob2)} should not be null at both");
        }

    }
}