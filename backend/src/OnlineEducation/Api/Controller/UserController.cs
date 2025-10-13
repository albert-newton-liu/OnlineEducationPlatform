using Microsoft.AspNetCore.Mvc;
using OnlineEducation.Api.Request;
using OnlineEducation.Api.Response;
using OnlineEducation.Model;
using OnlineEducation.Service;
using OnlineEducation.Utils;
using System.Security.Claims; // Added for ClaimTypes if used internally
using System; // Added for Exception

namespace OnlineEducation.Api.Controller;

/// <summary>
/// Controller for managing user accounts, including registration, login, and retrieval of user data.
/// Supports Admin, Student, and Teacher roles.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly IJwtTokenHelper _jwtTokenHelper;

    /// <summary>
    /// Initializes a new instance of the <see cref="UsersController"/> class.
    /// </summary>
    /// <param name="userService">The service responsible for user business logic and persistence.</param>
    /// <param name="jwtTokenHelper">The helper service for JWT token generation.</param>
    public UsersController(IUserService userService, IJwtTokenHelper jwtTokenHelper)
    {
        _userService = userService;
        _jwtTokenHelper = jwtTokenHelper;
    }

    /// <summary>
    /// Registers a new user with the Admin role.
    /// </summary>
    /// <param name="request">The request containing the new Admin's registration details.</param>
    /// <returns>
    /// A 201 Created result with the created Admin object on success,
    /// a 400 Bad Request if the model state is invalid or a business rule is violated (e.g., duplicate username),
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
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

    /// <summary>
    /// Registers a new user with the Student role.
    /// </summary>
    /// <param name="request">The request containing the new Student's registration details.</param>
    /// <returns>
    /// A 201 Created result with the created Student object on success,
    /// a 400 Bad Request if the model state is invalid or a business rule is violated,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
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

    /// <summary>
    /// Registers a new user with the Teacher role.
    /// </summary>
    /// <param name="request">The request containing the new Teacher's registration details.</param>
    /// <returns>
    /// A 201 Created result with the created Teacher object on success,
    /// a 400 Bad Request if the model state is invalid or a business rule is violated,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
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

    /// <summary>
    /// Authenticates a user and generates a JWT token upon successful login.
    /// </summary>
    /// <param name="request">The login request containing username and password.</param>
    /// <returns>
    /// A 200 OK result with the user details and JWT token on success,
    /// a 401 Unauthorized if the credentials are invalid,
    /// a 400 Bad Request if the model state is invalid,
    /// or a 500 Internal Server Error for unexpected exceptions.
    /// </returns>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserLoginResponse), StatusCodes.Status200OK)]
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
            var token = _jwtTokenHelper.GenerateToken(loggedInUser.UserId, loggedInUser.Role);
            response.Token = token;

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

    /// <summary>
    /// Retrieves a user's details by their unique identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the user.</param>
    /// <returns>
    /// A 200 OK result containing the User object if found,
    /// or a 404 Not Found if no user matches the ID.
    /// </returns>
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

    /// <summary>
    /// Retrieves a paginated list of users. This endpoint is typically restricted to Admins.
    /// </summary>
    /// <param name="paginationParams">Parameters for pagination, including page number and page size.</param>
    /// <returns>
    /// A 200 OK result containing the paginated list of user summaries,
    /// a 400 Bad Request if pagination parameters are invalid,
    /// a 401 Unauthorized, or a 403 Forbidden.
    /// </returns>
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