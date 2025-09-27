using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Core;
using OnlineEducation.Model;

namespace OnlineEducation.Service;



public class AnnouncementService : IAnnouncementService
{


    private readonly IAnnouncementCoreService _announcementCoreService;

    public AnnouncementService(IAnnouncementCoreService announcementCoreService)
    {
        _announcementCoreService = announcementCoreService;
    }

    public async Task AddAnnouncement(Announcement announcement)
    {
        await _announcementCoreService.AddAnnouncement(announcement);
    }

    public async Task<PaginatedResult<Announcement>?> GetPaginatedAsync(PaginationParams paginationParams)
    {
        return await _announcementCoreService.GetPaginatedAsync(paginationParams);
    }


}