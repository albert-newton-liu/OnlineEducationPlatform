using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

/// <summary>
/// Repository for managing announcement data access in the database.
/// Implements methods for retrieving and manipulating <see cref="AnnouncementDO"/> entities.
/// </summary>
public class AnnouncementRepository : Repository<AnnouncementDO>, IAnnouncementRepository
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnnouncementRepository"/> class.
    /// </summary>
    /// <param name="context">The database context to use for data access.</param>
    public AnnouncementRepository(ApplicationDbContext context) : base(context) { }

    /// <summary>
    /// Retrieves an announcement by its unique identifier asynchronously.
    /// </summary>
    /// <param name="id">The unique identifier of the announcement.</param>
    /// <returns>The <see cref="AnnouncementDO"/> if found; otherwise, null.</returns>
    public override async Task<AnnouncementDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.AnnouncementId == id);
    }
}