using Microsoft.AspNetCore.Mvc;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineEducation.Api.Controller;

[Route("api/[controller]")]
[ApiController]
public class UploadController : ControllerBase
{
    private const string UPLOAD_DIR = "uploads";
    private readonly IWebHostEnvironment _env;

    public UploadController(IWebHostEnvironment env)
    {
        _env = env;
    }


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
                accessUrl = $"{Request.Scheme}://{Request.Host}/files/{uniqueFileName}"
            });
        }
        catch (IOException e)
        {
            return StatusCode(500, $"upload file fail: {e.Message}");
        }
    }
}
