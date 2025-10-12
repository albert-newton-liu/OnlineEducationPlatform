using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

public class AnnouncementRepository : Repository<AnnouncementDO>, IAnnouncementRepository
{

    public AnnouncementRepository(ApplicationDbContext context) : base(context) { }


    public override async Task<AnnouncementDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.AnnouncementId == id);
    }


}