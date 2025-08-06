using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using OnlineEducation.Service;

namespace OnlineEducation.Api.Controller;

[ApiController]
[Route("api/[controller]")]
public class LessonController : ControllerBase
{

    private readonly ILessonService _lessonService;

    public LessonController(ILessonService service)
    {
        _lessonService = service;
    }

    [HttpPost("addlesson")]
    [ProducesResponseType(typeof(AddLessonRequest), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterAdmin([FromBody] AddLessonRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            string lessonId = await _lessonService.Add(request);
            return Ok(lessonId);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during admin registration." });
        }
    }


    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Lesson>> GetById(string id)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            Lesson lesson = await _lessonService.QueryByLessonId(id);
            return Ok(lesson);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during admin registration." });
        }

    }


    [HttpGet()]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<BasicLessonResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<BasicLessonResponse>>> GetPaginated([FromQuery] PaginationParams paginationParams)
    {
        if (paginationParams == null)
        {
            paginationParams = new PaginationParams();
        }
        if (paginationParams.PageNumber < 1 || paginationParams.PageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        var paginatedLessons = await _lessonService.GetPaginatedBasicLessonAsync(paginationParams);

        return Ok(paginatedLessons);
    }

}