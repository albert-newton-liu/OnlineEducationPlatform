namespace OnlineEducation.Utils;

public class AssertUtil
{
    public static void AssertNotNull(object? obj)
    {
        ArgumentNullException.ThrowIfNull(obj);
    }

     public static void AssertBothNotNull(object? obj1, object? ob2)
    {
        if (obj1 == null && ob2 == null)
        {
            throw new ArgumentException($"{nameof(obj1)} and {nameof(ob2)} should not be null at both");
        }
        
    }
}