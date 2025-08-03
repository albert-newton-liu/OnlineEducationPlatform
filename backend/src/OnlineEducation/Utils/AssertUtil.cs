namespace OnlineEducation.Utils;

public class AssertUtil
{
    public static void AssertNotNull(object? obj)
    {
        ArgumentNullException.ThrowIfNull(obj);

    }
}