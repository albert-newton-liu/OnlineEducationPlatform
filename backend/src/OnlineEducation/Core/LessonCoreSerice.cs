using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Core;

public class LessonCoreSerice : ILessonCoreSerice
{

    private readonly ILessonRepository _lessonRepository;
    private readonly ILessonPageRepository _lessonPageRepository;
    private readonly ILessonPageElementRepository _lessonPageElementRepository;
    private ApplicationDbContext _dbContext;


    public LessonCoreSerice(
        ILessonRepository lessonRepository,
        ILessonPageRepository lessonPageRepository,
        ILessonPageElementRepository lessonPageElementRepository,
        ApplicationDbContext context)
    {
        _lessonRepository = lessonRepository;
        _lessonPageRepository = lessonPageRepository;
        _lessonPageElementRepository = lessonPageElementRepository;
        _dbContext = context;
    }

    public async Task DeleteLesson(string lessonId)
    {
        List<LessonPageDO>? lessonPageDOs = await _lessonPageRepository.QueryByLessonIdAsync(lessonId);
        if (lessonPageDOs == null)
        {
            return;
        }

        lessonPageDOs.ForEach(async x =>
        {
            await _lessonPageElementRepository.DeleteByPageIdAsync(x.PageId);
            await _lessonPageRepository.removeById(x.PageId);
        });

        await _lessonRepository.removeById(lessonId);
        await _lessonPageRepository.SaveChangesAsync();
    }

    public async Task DeletePage(string pageId)
    {
        await _lessonPageElementRepository.DeleteByPageIdAsync(pageId);
        await _lessonPageRepository.removeById(pageId);
        await _lessonPageRepository.SaveChangesAsync();
    }

    public async Task<string> InsertLesson(Lesson lesson)
    {
        foreach (var lessonPage in lesson.Pages)
        {
            foreach (var element in lessonPage.Elements)
            {
                LessonPageElementDO elementDO = convertToDO(element);
                await _lessonPageElementRepository.AddAsync(elementDO);

            }
            LessonPageDO pageDO = convertToDO(lessonPage);
            await _lessonPageRepository.AddAsync(pageDO);

        }

        LessonDO lessonDO = convertToDO(lesson);
        await _lessonRepository.AddAsync(lessonDO);
        await _lessonRepository.SaveChangesAsync();

        return lessonDO.LessonId;
    }

    public async Task<LessonPage> InsertPage(LessonPage lessonPage)
    {
        foreach (var item in lessonPage.Elements)
        {
            LessonPageElementDO elementDO = convertToDO(item);
            await _lessonPageElementRepository.AddAsync(elementDO);
        }

        LessonPageDO pageDO = convertToDO(lessonPage);
        await _lessonPageRepository.AddAsync(pageDO);
        await _lessonPageRepository.SaveChangesAsync();

        return await QueryByPageId(lessonPage.PageId);

    }

    public async Task<Lesson> QueryByLessonId(string lessonId)
    {
        LessonDO? lessonDO = await _lessonRepository.GetByIdAsync(lessonId);
        ArgumentNullException.ThrowIfNull(lessonDO);

        List<LessonPageDO>? lessonPageDOs = await _lessonPageRepository.QueryByLessonIdAsync(lessonId);
        ArgumentNullException.ThrowIfNull(lessonPageDOs);

        List<LessonPage> lessonPages = [];
        foreach (var item in lessonPageDOs)
        {
            LessonPage lessonPage = await QueryByPageId(item.PageId);
            lessonPages.Add(lessonPage);
        }

        lessonPages = [.. lessonPages.OrderBy(x => x.PageNumber)];

        Lesson lesson = ConvertToBO(lessonDO);
        lesson.Pages = lessonPages;
        return lesson;
    }

    public async Task<LessonPage> QueryByPageId(string pageId)
    {
        LessonPageDO? pageDO = await _lessonPageRepository.GetByIdAsync(pageId);
        ArgumentNullException.ThrowIfNull(pageDO);
        LessonPage lessonPage = ConvertToBO(pageDO);

        List<LessonPageElementDO>? elementDOs = await _lessonPageElementRepository.QueryByPageIdAsync(pageId);
        if (elementDOs != null && elementDOs.Count > 0)
        {
            lessonPage.Elements = [.. elementDOs.Select(x => ConvertToBO(x))];
        }

        return lessonPage;
    }

    public Task<LessonPage> UpdatePage(LessonPage lessonPage)
    {
        throw new NotImplementedException();
    }

    public async Task<PaginatedResult<LessonDO>> GetPaginatedBaseUsersAsync(PaginationParams paginationParams)
    {
        var query = _dbContext.LessonDOs.AsQueryable();
        var totalCount = await query.CountAsync();
        var lessons = await query.OrderByDescending(l => l.CreatedAt)
         .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
         .Take(paginationParams.PageSize)
         .ToListAsync();

        return new PaginatedResult<LessonDO>(lessons, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }


    public async Task Approve(string lessonId)
    {
        LessonDO? lesson = await _lessonRepository.GetByIdAsync(lessonId);
        ArgumentNullException.ThrowIfNull(lesson);
        lesson.IsPublished = true;
        await _lessonRepository.SaveChangesAsync();
    }



    private LessonPageElementDO convertToDO(LessonPageElement element)
    {

        return new LessonPageElementDO
        {
            ElementId = element.ElementId,
            PageId = element.PageId,
            ElementType = (byte)element.ElementType,
            ContentText = element.ContentText,
            ContentUrl = element.ContentUrl,
            ElementMetadata = element.ElementMetadata,
            CreatedAt = element.CreatedAt

        };
    }

    private LessonPageElement ConvertToBO(LessonPageElementDO element)
    {

        return new LessonPageElement
        {
            ElementId = element.ElementId,
            PageId = element.PageId,
            ElementType = (ElementTypeEnum)element.ElementType,
            ContentText = element.ContentText,
            ContentUrl = element.ContentUrl,
            ElementMetadata = element.ElementMetadata,
            CreatedAt = element.CreatedAt

        };
    }


    private LessonPageDO convertToDO(LessonPage page)
    {

        return new LessonPageDO
        {
            PageId = page.PageId,
            LessonId = page.LessonId,
            PageNumber = page.PageNumber,
            PageLayout = page.PageLayout,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }

    private LessonPage ConvertToBO(LessonPageDO page)
    {

        return new LessonPage
        {
            PageId = page.PageId,
            LessonId = page.LessonId,
            PageNumber = page.PageNumber,
            PageLayout = page.PageLayout,
            CreatedAt = page.CreatedAt,
            UpdatedAt = page.UpdatedAt
        };
    }

    private LessonDO convertToDO(Lesson lesson)
    {

        return new LessonDO
        {
            LessonId = lesson.LessonId,
            TeacherId = lesson.TeacherId,
            Title = lesson.Title,
            Description = lesson.Description,
            DifficultyLevel = lesson.DifficultyLevel,
            ThumbnailUrl = lesson.ThumbnailUrl,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            IsPublished = lesson.IsPublished,
            AdminReviewedAt = lesson.AdminReviewedAt
        };
    }

    private Lesson ConvertToBO(LessonDO lesson)
    {

        return new Lesson
        {
            LessonId = lesson.LessonId,
            TeacherId = lesson.TeacherId,
            Title = lesson.Title,
            Description = lesson.Description,
            DifficultyLevel = lesson.DifficultyLevel,
            ThumbnailUrl = lesson.ThumbnailUrl,
            CreatedAt = lesson.CreatedAt,
            UpdatedAt = lesson.UpdatedAt,
            IsPublished = lesson.IsPublished,
            AdminReviewedAt = lesson.AdminReviewedAt
        };
    }

}