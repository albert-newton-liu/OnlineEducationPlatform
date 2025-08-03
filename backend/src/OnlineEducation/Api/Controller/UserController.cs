using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using OnlineEducation.Service;

namespace OnlineEducation.Api.Controller;


[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpPost("register/admin")]
    [ProducesResponseType(typeof(Admin), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterAdmin([FromBody] AdminAddRequst request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            Admin createdAdmin = await _userService.AddAdmin(request);
            return CreatedAtAction(nameof(RegisterAdmin), new { id = createdAdmin.UserId }, createdAdmin);
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

    [HttpPost("register/student")]
    [ProducesResponseType(typeof(Student), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterStudent([FromBody] StudentAddRequst request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            Student createdStudent = await _userService.AddStudent(request);
            return CreatedAtAction(nameof(RegisterStudent), new { id = createdStudent.UserId }, createdStudent);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during student registration." });
        }
    }

    [HttpPost("register/teacher")]
    [ProducesResponseType(typeof(Teacher), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> RegisterTeacher([FromBody] TeacherAddRequst request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            Teacher createdTeacher = await _userService.AddTeacher(request);
            return CreatedAtAction(nameof(RegisterTeacher), new { id = createdTeacher.UserId }, createdTeacher);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during teacher registration." });
        }
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            User loggedInUser = await _userService.Login(request.Username, request.Password);

            UserLoginResponse response = new UserLoginResponse();
            response.UserId = loggedInUser.UserId;
            response.Username = loggedInUser.Username;
            response.Role = loggedInUser.Role;
            if (loggedInUser is Admin admin)
            {
                response.Permissions = admin.Permissions;
            }
            response.Token = "token";

            return Ok(response);
        }
        catch (UnauthorizedAccessException ex)
        {
            return Unauthorized(new { message = ex.Message });
        }
        catch (Exception)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "An unexpected error occurred during login." });
        }
    }

    [HttpGet("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(User))]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<User>> GetById(string id)
    {
        var user = await _userService.QueryById(id);

        if (user == null)
        {
            return NotFound($"User with ID '{id}' not found.");
        }

        return Ok(user);
    }
    
    [HttpGet()] 
    [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(PaginatedResult<UserQueryResponse>))]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<ActionResult<PaginatedResult<UserQueryResponse>>> GetPaginated([FromQuery] PaginationParams paginationParams)
    {
        if (paginationParams == null)
        {
            paginationParams = new PaginationParams();
        }
        if (paginationParams.PageNumber < 1 || paginationParams.PageSize < 1)
        {
            return BadRequest("PageNumber and PageSize must be greater than 0.");
        }

        var paginatedUsers = await _userService.GetPaginatedUsersAsync(paginationParams);

        return Ok(paginatedUsers);
    }

}