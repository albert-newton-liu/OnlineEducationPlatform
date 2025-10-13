using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Data.Dao;
using OnlineEducation.Data.Repository;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Core;

/// <summary>
/// Core service for managing lessons in the Online Education Platform.
/// Provides methods for inserting, updating, deleting, querying, and approving lessons and lesson pages.
/// </summary>
public class LessonCoreSerice : ILessonCoreSerice
{
    private readonly ILessonRepository _lessonRepository;
    private readonly ILessonPageRepository _lessonPageRepository;
    private readonly ILessonPageElementRepository _lessonPageElementRepository;
    private ApplicationDbContext _dbContext;

    /// <summary>
    /// Initializes a new instance of the <see cref="LessonCoreSerice"/> class.
    /// </summary>
    /// <param name="lessonRepository">Repository for lesson data.</param>
    /// <param name="lessonPageRepository">Repository for lesson page data.</param>
    /// <param name="lessonPageElementRepository">Repository for lesson page element data.</param>
    /// <param name="context">Database context.</param>
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

    /// <summary>
    /// Deletes a lesson and all its associated pages and elements by lesson ID.
    /// </summary>
    /// <param name="lessonId">The unique identifier of the lesson to delete.</param>
    public async Task DeleteLesson(string lessonId)
    {
        List<LessonPageDO>? lessonPageDOs = await _lessonPageRepository.QueryByLessonIdAsync(lessonId);
        if (lessonPageDOs == null)
        {
            return;
        }

        foreach (var x in lessonPageDOs)
        {
            await _lessonPageElementRepository.DeleteByPageIdAsync(x.PageId);
            await _lessonPageRepository.removeById(x.PageId);
        }

        await _lessonRepository.removeById(lessonId);
        await _lessonPageRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Deletes a lesson page and all its elements by page ID.
    /// </summary>
    /// <param name="pageId">The unique identifier of the lesson page to delete.</param>
    public async Task DeletePage(string pageId)
    {
        await _lessonPageElementRepository.DeleteByPageIdAsync(pageId);
        await _lessonPageRepository.removeById(pageId);
        await _lessonPageRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Inserts a new lesson along with its pages and elements.
    /// </summary>
    /// <param name="lesson">The lesson to insert.</param>
    /// <returns>The unique identifier of the inserted lesson.</returns>
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

    /// <summary>
    /// Inserts a new page into a lesson, including its elements.
    /// </summary>
    /// <param name="lessonPage">The lesson page to insert.</param>
    /// <returns>The inserted <see cref="LessonPage"/> object.</returns>
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

    /// <summary>
    /// Retrieves lesson details by lesson ID, including its pages and elements.
    /// </summary>
    /// <param name="lessonId">The unique identifier of the lesson.</param>
    /// <returns>The <see cref="Lesson"/> object.</returns>
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

    /// <summary>
    /// Retrieves lesson page details by page ID, including its elements.
    /// </summary>
    /// <param name="pageId">The unique identifier of the lesson page.</param>
    /// <returns>The <see cref="LessonPage"/> object.</returns>
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

    /// <summary>
    /// Updates lesson page details.
    /// </summary>
    /// <param name="lessonPage">The lesson page with updated information.</param>
    /// <returns>The updated <see cref="LessonPage"/> object.</returns>
    public Task<LessonPage> UpdatePage(LessonPage lessonPage)
    {
        throw new NotImplementedException();
    }

    /// <summary>
    /// Retrieves a paginated list of lessons based on pagination parameters and query conditions.
    /// </summary>
    /// <param name="paginationParams">Pagination parameters for the query.</param>
    /// <param name="conditon">Lesson query conditions.</param>
    /// <returns>A <see cref="PaginatedResult{LessonDO}"/> containing the lessons for the requested page.</returns>
    public async Task<PaginatedResult<LessonDO>> GetPaginatedBaseUsersAsync(PaginationParams paginationParams, LessonQueryConditon conditon)
    {
        var query = _dbContext.LessonDOs.AsQueryable();
        if (conditon != null)
        {
            if (conditon.MustPublished)
            {
                query = query.Where(x => x.IsPublished);
            }
            if (conditon.TheacherId != null)
            {
                query = query.Where(x => x.TeacherId == conditon.TheacherId);
            }
        }
        var totalCount = await query.CountAsync();
        var lessons = await query.OrderByDescending(l => l.CreatedAt)
         .Skip((paginationParams.PageNumber - 1) * paginationParams.PageSize)
         .Take(paginationParams.PageSize)
         .ToListAsync();

        return new PaginatedResult<LessonDO>(lessons, totalCount, paginationParams.PageNumber, paginationParams.PageSize);
    }

    /// <summary>
    /// Approves a lesson by its unique identifier, marking it as published.
    /// </summary>
    /// <param name="lessonId">The unique identifier of the lesson to approve.</param>
    public async Task Approve(string lessonId)
    {
        LessonDO? lesson = await _lessonRepository.GetByIdAsync(lessonId);
        ArgumentNullException.ThrowIfNull(lesson);
        lesson.IsPublished = true;
        await _lessonRepository.SaveChangesAsync();
    }

    /// <summary>
    /// Converts a <see cref="LessonPageElement"/> business object to a <see cref="LessonPageElementDO"/> data object.
    /// </summary>
    /// <param name="element">The lesson page element business object.</param>
    /// <returns>The corresponding data object.</returns>
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

    /// <summary>
    /// Converts a <see cref="LessonPageElementDO"/> data object to a <see cref="LessonPageElement"/> business object.
    /// </summary>
    /// <param name="element">The lesson page element data object.</param>
    /// <returns>The corresponding business object.</returns>
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

    /// <summary>
    /// Converts a <see cref="LessonPage"/> business object to a <see cref="LessonPageDO"/> data object.
    /// </summary>
    /// <param name="page">The lesson page business object.</param>
    /// <returns>The corresponding data object.</returns>
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

    /// <summary>
    /// Converts a <see cref="LessonPageDO"/> data object to a <see cref="LessonPage"/> business object.
    /// </summary>
    /// <param name="page">The lesson page data object.</param>
    /// <returns>The corresponding business object.</returns>
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

    /// <summary>
    /// Converts a <see cref="Lesson"/> business object to a <see cref="LessonDO"/> data object.
    /// </summary>
    /// <param name="lesson">The lesson business object.</param>
    /// <returns>The corresponding data object.</returns>
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

    /// <summary>
    /// Converts a <see cref="LessonDO"/> data object to a <see cref="Lesson"/> business object.
    /// </summary>
    /// <param name="lesson">The lesson data object.</param>
    /// <returns>The corresponding business object.</returns>
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

    /// <summary>
    /// Retrieves a list of lessons by their unique identifiers.
    /// </summary>
    /// <param name="lessonIDs">The list of lesson IDs to query.</param>
    /// <returns>A list of <see cref="Lesson"/> objects.</returns>
    public async Task<List<Lesson>> QueryByLessonIds(List<string> lessonIDs)
    {
        IEnumerable<LessonDO> lessonDOs = await _lessonRepository.GetAllAsync();
        lessonDOs = lessonDOs.Where(u => lessonIDs.Contains(u.LessonId));
        return [.. lessonDOs.Select(x => ConvertToBO(x))];
    }
}