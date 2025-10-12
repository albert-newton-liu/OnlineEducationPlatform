using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;

namespace OnlineEducation.Service;

public interface IAnnouncementService
{
    Task AddAnnouncement(Announcement announcement);

    public Task<PaginatedResult<Announcement>?> GetPaginatedAsync(PaginationParams paginationParams);
}