using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Core;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

/// <summary>
/// Service for managing announcements in the online education platform.
/// Implements the <see cref="IAnnouncementService"/> interface.
/// </summary>
public class AnnouncementService : IAnnouncementService
{

    private readonly IAnnouncementCoreService _announcementCoreService;

    public AnnouncementService(IAnnouncementCoreService announcementCoreService)
    {
        _announcementCoreService = announcementCoreService;
    }

    /// <summary>
    /// Adds a new announcement to the platform.
    /// </summary>
    public async Task AddAnnouncement(Announcement announcement)
    {
        await _announcementCoreService.AddAnnouncement(announcement);
    }

    /// <summary>
    /// Get Paginated Announcements from the platform.
    /// </summary>
    public async Task<PaginatedResult<Announcement>?> GetPaginatedAsync(PaginationParams paginationParams)
    {
        return await _announcementCoreService.GetPaginatedAsync(paginationParams);
    }


}