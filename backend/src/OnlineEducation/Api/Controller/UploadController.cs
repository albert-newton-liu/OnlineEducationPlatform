using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;

namespace OnlineEducation.Api.Controller;

/// <summary>
/// Controller for handling file upload operations.
/// </summary>
[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    private const string UPLOAD_DIR = "uploads";
    private readonly IWebHostEnvironment _env;

    /// <summary>
    /// Initializes a new instance of the <see cref="UploadController"/> class.
    /// </summary>
    /// <param name="env">The web hosting environment, used to determine the content root path for file storage.</param>
    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }


    /// <summary>
    /// Uploads a single file to the server's designated upload directory.
    /// </summary>
    /// <param name="file">The file content sent via multipart/form-data.</param>
    /// <returns>
    /// A 200 OK result with the saved file's unique name, relative path, and accessible URL on success,
    /// a 400 Bad Request if no file is provided,
    /// or a 500 Internal Server Error if the file saving process fails.
    /// </returns>
    [HttpPost("upload")]
    [DisableRequestSizeLimit]
    [Consumes("multipart/form-data")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> UploadFile([FromForm] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("Please select a file to upload.");
        }

        try
        {
            var uploadPath = Path.Combine(_env.ContentRootPath, UPLOAD_DIR);

            if (!Directory.Exists(uploadPath))
            {
                Directory.CreateDirectory(uploadPath);
            }

            var fileExtension = Path.GetExtension(file.FileName);
            var uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
            var filePath = Path.Combine(uploadPath, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var savedRelativePath = Path.Combine(UPLOAD_DIR, uniqueFileName).Replace('\\', '/');

            return Ok(new
            {
                filename = uniqueFileName,
                savedPath = savedRelativePath,
                accessUrl = $"/files/{uniqueFileName}"
            });
        }
        catch (IOException e)
        {
            return StatusCode(500, $"upload file fail: {e.Message}");
        }
    }
}