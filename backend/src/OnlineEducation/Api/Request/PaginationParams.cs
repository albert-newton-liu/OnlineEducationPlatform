// In a new folder like OnlineEducation.Shared.Request (or your existing Request folder)
namespace OnlineEducation.Api.Request;

public class PaginationParams
{
    // The default page size if not specified by the client
    private const int MaxPageSize = 100; // Define a max page size to prevent abuse
    private int _pageSize = 10; // Default page size

    /// <summary>
    /// The current page number (1-based).
    /// </summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>
    /// The number of items to return per page.
    /// Clamped between 1 and MaxPageSize.
    /// </summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = (value > MaxPageSize) ? MaxPageSize : value;
    }
}