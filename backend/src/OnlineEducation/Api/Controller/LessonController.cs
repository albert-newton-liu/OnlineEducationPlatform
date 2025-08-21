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

    [HttpPost("approve/{LessonId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Approve(string LessonId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            string token = GetToken();
            if (string.IsNullOrEmpty(token))
            {
                return Unauthorized(new { message = "Authorization token is missing or invalid." });
            }

            string AdminId = "115f62ab-82a3-41d4-af0c-1f02d449f043";

            await _lessonService.Approve(LessonId, AdminId);
            return Ok();
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
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Lesson))]
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
        string token = GetToken();
        if (string.IsNullOrEmpty(token))
        {
            return Unauthorized(new { message = "Authorization token is missing or invalid." });
        }

        var str = token.Split(',');


        if (paginationParams == null)
        {
            paginationParams = new PaginationParams();
        }
        if (paginationParams.PageNumber < 1 || paginationParams.PageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        LessonQueryConditon conditon = new();
        if (((int)UserRole.Student).ToString() == str[1])
        {
            conditon.MustPublished = true;
        }
        if (((int)UserRole.Teacher).ToString() == str[1])
        {
            conditon.TheacherId = str[2];
        }


        var paginatedLessons = await _lessonService.GetPaginatedBasicLessonAsync(paginationParams, conditon);

        return Ok(paginatedLessons);
    }

    private string GetToken()
    {
        string token = string.Empty;
        if (Request.Headers.TryGetValue("Authorization", out Microsoft.Extensions.Primitives.StringValues value))
        {
            var authHeader = value.FirstOrDefault();
            if (authHeader != null && authHeader.StartsWith("Bearer "))
            {
                token = authHeader.Substring("Bearer ".Length).Trim();
            }
        }
        return token;
    }

}