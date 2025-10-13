using Microsoft.EntityFrameworkCore;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;

namespace OnlineEducation.Core;

/// <summary>
/// Core service for managing announcements in the Online Education Platform.
/// Provides methods to add announcements and retrieve paginated lists of active announcements.
/// </summary>
public class AnnouncementCoreService : IAnnouncementCoreService
{
    // Repository for accessing announcement data
    protected readonly IAnnouncementRepository _announcementRepository;

    // Database context for direct data access
    private readonly ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnnouncementCoreService"/> class.
    /// </summary>
    /// <param name="announcementRepository">The repository for announcement data access.</param>
    /// <param name="dbContext">The database context for direct data access.</param>
    public AnnouncementCoreService(IAnnouncementRepository announcementRepository, ApplicationDbContext dbContext)
    {
        _announcementRepository = announcementRepository;
        _dbContext = dbContext;
    }

    /// <summary>
    /// Adds a new announcement to the system.
    /// </summary>
    /// <param name="announcement">The announcement to add.</param>
    /// <exception cref="ArgumentNullException">Thrown if the announcement is null.</exception>
    public async Task AddAnnouncement(Announcement announcement)
    {
        if (announcement == null)
        {
            throw new ArgumentNullException(nameof(announcement));
        }

        AnnouncementDO announcementDO = new()
        {
            AnnouncementId = Guid.NewGuid().ToString(),
            Title = announcement.Title,
            Content = announcement.Content,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsActive = true
        };


        await _announcementRepository.AddAsync(announcementDO);
        await _announcementRepository.SaveChangesAsync();

    }

    /// <summary>
    /// Retrieves a paginated list of active announcements.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters for the query.</param>
    /// <returns>
    /// A <see cref="PaginatedResult{Announcement}"/> containing the announcements for the requested page,
    /// or null if no announcements are found.
    /// </returns>
    public async Task<PaginatedResult<Announcement>?> GetPaginatedAsync(PaginationParams paginationParams)
    {
        var query = _dbContext.AnnouncementDOs
            //  Use AsNoTracking() for read-only operations to improve performance.
            .AsNoTracking()
            // Filter: Only select announcements that are active (IsActive = true).
            .Where(x => x.IsActive)
            // Convert to AsQueryable to allow further chained LINQ operations.
            .AsQueryable();

        // Calculate the total number of records that match the filter (IsActive = true).
        // This is needed for the PaginatedResult metadata.
        var totalCount = await query.CountAsync();

        // Execute the final query to fetch the specific page of data.
        var announcements = await query
            // Ordering: Sort by creation date in descending order to get the newest first.
            // Consistent ordering is crucial for reliable pagination.
            .OrderByDescending(u => u.CreatedAt)
            // Skip: Calculate and skip the announcements from previous pages.
            // Formula: (PageNumber - 1) * PageSize
            .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
            // Take: Limit the results to the requested page size.
            .Take(paginationParams.PageSize)
            // Execute the query against the database and materialize the results.
            .ToListAsync();

        // Check: If the query returned no results for the requested page, return null.
        if (announcements == null || announcements.Count == 0)
        {
            return null;
        }

        // Mapping: Transform the internal data objects (DOs) into the public DTO (Announcement).
        var mappedAnnouncements = announcements.Select(a =>
            new Announcement
            {
                AnnouncementId = a.AnnouncementId,
                Title = a.Title,
                Content = a.Content
            }
        ).ToList();

        // Return: Package the results, total count, and pagination parameters into the final PaginatedResult.
        return new PaginatedResult<Announcement>(mappedAnnouncements, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }
}