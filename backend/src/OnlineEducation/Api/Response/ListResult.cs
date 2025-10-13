namespace OnlineEducation.Api.Response;

/// <summary>
/// Generic response for a list of items.
/// </summary>
public class ListResult<T> : BaseResponse
{
    /// <summary>
    /// The total number of items available.
    /// </summary>
    public IEnumerable<T> Items { get; set; } = new List<T>();

}