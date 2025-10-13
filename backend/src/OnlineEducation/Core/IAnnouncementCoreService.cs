using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Core;

/// <summary>
/// Core service interface for managing announcements in the Online Education Platform.
/// Provides methods to add announcements and retrieve paginated lists of active announcements.
/// </summary>
public interface IAnnouncementCoreService
{
    /// <summary>
    /// Adds a new announcement to the system.
    /// </summary>
    /// <param name="announcement">The announcement to add.</param>
    Task AddAnnouncement(Announcement announcement);

    /// <summary>
    /// Retrieves a paginated list of active announcements.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters for the query.</param>
    /// <returns>
    /// A <see cref="PaginatedResult{Announcement}"/> containing the announcements for the requested page,
    /// or null if no announcements are found.
    /// </returns>
    Task<PaginatedResult<Announcement>?> GetPaginatedAsync(PaginationParams paginationParams);
}