using Microsoft.EntityFrameworkCore;
using OnlineEducation.Data.Dao;

namespace OnlineEducation.Data.Repository;

public class LessonRepository : Repository<LessonDO>, ILessonRepository
{
    public LessonRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<LessonDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.LessonId == id);
    }

}

public class LessonPageRepository : Repository<LessonPageDO>, ILessonPageRepository
{
    public LessonPageRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<LessonPageDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.PageId == id);
    }

    public async Task DeleteByByLessonIdAsync(string LessonId)
    {
        var pagesToDelete = await _dbSet.Where(page => page.LessonId == LessonId).ToListAsync();

        if (pagesToDelete.Count != 0)
        {
            _dbSet.RemoveRange(pagesToDelete);
        }
    }

    public async Task<List<LessonPageDO>?> QueryByLessonIdAsync(string LessonId)
    {
        return await _dbSet.Where(page => page.LessonId == LessonId).ToListAsync();

    }
}
public class LessonPageElementRepository : Repository<LessonPageElementDO>, ILessonPageElementRepository
{
    public LessonPageElementRepository(ApplicationDbContext context) : base(context) { }

    public override async Task<LessonPageElementDO?> GetByIdAsync(string id)
    {
        return await _dbSet.FirstOrDefaultAsync(u => u.ElementId == id);
    }

    public async Task DeleteByPageIdAsync(string PageId)
    {
        var elementsToDelete = await _dbSet.Where(element => element.PageId == PageId).ToListAsync();

        if (elementsToDelete.Count != 0)
        {
            _dbSet.RemoveRange(elementsToDelete);
        }
    }

    public async Task<List<LessonPageElementDO>?> QueryByPageIdAsync(string PageId)
    {
        return await _dbSet.Where(element => element.PageId == PageId).ToListAsync();
    }
}