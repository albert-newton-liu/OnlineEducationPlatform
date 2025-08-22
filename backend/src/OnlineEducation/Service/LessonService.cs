using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Core;
using OnlineEducation.Data.Dao;
using OnlineEducation.Model;
using OnlineEducation.Utils;

namespace OnlineEducation.Service;

public class LessonService : ILessonService
{
    private readonly ILessonCoreSerice _lessonCoreSerice;

    private readonly IUserCoreService _userCoreService;

    public LessonService(ILessonCoreSerice lessonCoreSerice, IUserCoreService userCoreService)
    {
        _lessonCoreSerice = lessonCoreSerice;
        _userCoreService = userCoreService;
    }

    public async Task<string> Add(AddLessonRequest request)
    {
        Lesson lesson = buildLesson(request);
        return await _lessonCoreSerice.InsertLesson(lesson);
    }


    public async Task Approve(string LessonId, string AdminId)
    {
        User? user = await _userCoreService.GetByIdAsync<User>(AdminId);
        ArgumentNullException.ThrowIfNull(user);
        await _lessonCoreSerice.Approve(LessonId);
    }

    public async Task Delete(string lessonId, string teacherId)
    {
        Lesson lesson = await _lessonCoreSerice.QueryByLessonId(lessonId);
        ArgumentNullException.ThrowIfNull(lesson);
        if (lesson.TeacherId != teacherId)
        {
            throw new ArgumentException("Can not delete others lesson");
        }
        await _lessonCoreSerice.DeleteLesson(lessonId);
    }

    public async Task<PaginatedResult<BasicLessonResponse>> GetPaginatedBasicLessonAsync(PaginationParams paginationParams, LessonQueryConditon conditon)
    {
        PaginatedResult<LessonDO> paginatedResult = await _lessonCoreSerice.GetPaginatedBaseUsersAsync(paginationParams, conditon);
        ArgumentNullException.ThrowIfNull(paginatedResult);

        IEnumerable<LessonDO> Items = paginatedResult.Items;
        if (Items == null)
        {
            return new([], paginatedResult.TotalCount, paginatedResult.PageNumber, paginatedResult.PageSize);
        }

        IEnumerable<UserDO> userDOs = await _userCoreService.QueryUserByCondition(new QueryUserCondition() { Role = (byte?)UserRole.Teacher });
        if (userDOs == null || !userDOs.Any())
        {
            return new([], paginatedResult.TotalCount, paginatedResult.PageNumber, paginatedResult.PageSize);

        }

        var userMap = userDOs.ToDictionary(u => u.UserId, u => u.Username);

        IEnumerable<BasicLessonResponse> lessonResponses = [.. Items.Select(item => new BasicLessonResponse
        {
            LessonId = item.LessonId,
            Title = item.Title,
            Description = item.Description,
            DifficultyLevel = item.DifficultyLevel,
            IsPublished = item.IsPublished,
            Creator = userMap.GetValueOrDefault(item.TeacherId, "Unknown User"),
            CreatorId = item.TeacherId
        })];


        return new(lessonResponses, paginatedResult.TotalCount, paginatedResult.PageNumber, paginatedResult.PageSize); ;
    }

    public async Task<Lesson> QueryByLessonId(string lessonId)
    {
        return await _lessonCoreSerice.QueryByLessonId(lessonId);
    }

    private Lesson buildLesson(AddLessonRequest request)
    {
        var lesson = new Lesson
        {
            LessonId = Guid.NewGuid().ToString(),
            TeacherId = request.TeacherId,
            Title = request.Title,
            Description = request.Description,
            DifficultyLevel = request.DifficultyLevel,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            IsPublished = false,
            ThumbnailUrl = null,
            AdminReviewedAt = null
        };

        if (request.Pages != null && request.Pages.Count != 0)
        {
            lesson.Pages = [.. request.Pages.Select(addPage =>
            {
                var lessonPage = new LessonPage
                {
                    PageId = Guid.NewGuid().ToString(),
                    LessonId = lesson.LessonId,
                    PageNumber = addPage.PageNumber,
                    PageLayout = addPage.PageLayout,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow

                };

                if (addPage.Elements != null && addPage.Elements.Count != 0)
                {
                    lessonPage.Elements = [.. addPage.Elements.Select(addElement => new LessonPageElement
                    {
                        ElementId = Guid.NewGuid().ToString(),
                        PageId = lessonPage.PageId,
                        ElementType = (ElementTypeEnum)addElement.ElementType,
                        ContentText = addElement.ContentText,
                        ContentUrl = addElement.ContentUrl,
                        EleOrder = addElement.Order,
                        ElementMetadata = addElement.ElementMetadata,
                        CreatedAt = DateTime.UtcNow
                    })];
                }

                return lessonPage;
            })];
        }

        return lesson;

    }
}