using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Core;



public interface IAnnouncementCoreService
{
    Task AddAnnouncement(Announcement announcement);

    public Task<PaginatedResult<Announcement>?> GetPaginatedAsync(PaginationParams paginationParams);
}