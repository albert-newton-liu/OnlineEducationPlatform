using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using OnlineEducation.Service;
using OnlineEducation.Utils;

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
    public async Task<IActionResult> Addlesson([FromBody] AddLessonRequest request)
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

    [Authorize]
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
            var AdminId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ArgumentNullException.ThrowIfNull(AdminId);
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

    [HttpDelete("delete/{LessonId}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult> Delete(string LessonId)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            ArgumentNullException.ThrowIfNull(userId);
            await _lessonService.Delete(LessonId, userId);
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


        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst(ClaimTypes.Role)?.Value;


        if (paginationParams == null)
        {
            paginationParams = new PaginationParams();
        }
        if (paginationParams.PageNumber < 1 || paginationParams.PageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        LessonQueryConditon conditon = new();
        if (((int)UserRole.Student).ToString() == role)
        {
            conditon.MustPublished = true;
        }
        if (((int)UserRole.Teacher).ToString() == role)
        {
            conditon.TheacherId = userId;
        }


        var paginatedLessons = await _lessonService.GetPaginatedBasicLessonAsync(paginationParams, conditon);

        return Ok(paginatedLessons);
    }

    private string GetToken()
    {
        var authHeader = HttpContext.Request.Headers["Authorization"].FirstOrDefault();
        if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            throw new Exception("token is null");
        return authHeader.Substring("Bearer ".Length).Trim();
    }

}