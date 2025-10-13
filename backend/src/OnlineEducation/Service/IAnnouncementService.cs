using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

/// <summary>
/// Interface for managing announcements in the online education platform.
/// </summary>
public interface IAnnouncementService
{
    /// <summary>
    /// Adds a new announcement to the platform.
    /// </summary>
    Task AddAnnouncement(Announcement announcement);

    /// <summary>
    /// Get Paginated Announcements from the platform.
    /// </summary>
    public Task<PaginatedResult<Announcement>?> GetPaginatedAsync(PaginationParams paginationParams);
}