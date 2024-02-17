using Microsoft.AspNetCore.Mvc;
using VideosMvcApp.Models;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<HomeController> _logger;

    public HomeController(IWebHostEnvironment webHostEnvironment, ILogger<HomeController> logger)
    {
        _webHostEnvironment = webHostEnvironment;
        _logger = logger;
    }

    public IActionResult Index()
    {
        var videoFiles = GetVideoFiles();
        return View(videoFiles);
    }

    [HttpPost]
    public IActionResult Upload(List<IFormFile> files)
    {
        try
        {
            foreach (var file in files)
            {
                if (file.Length > 0 && Path.GetExtension(file.FileName).ToLower() == ".mp4")
                {
                    var fileName = $"{Guid.NewGuid().ToString()}_{file.FileName}";
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Media", fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        file.CopyTo(stream);
                    }
                }
                else
                {
                    // Handle invalid file
                }
            }

            var videoFiles = GetVideoFiles();
            return View("Index", videoFiles);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during file upload");
            ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
            if (_webHostEnvironment.IsDevelopment())
            {
                ViewData["ErrorDetails"] = ex.ToString();
            }
            return View("Error");
        }
    }

    public IActionResult Play(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Media", fileName);
            return PhysicalFile(filePath, "video/mp4", enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during video playback");
            ViewBag.ErrorMessage = "An error occurred while processing your request. Please try again.";
            if (_webHostEnvironment.IsDevelopment())
            {
                ViewData["ErrorDetails"] = ex.ToString();
            }
            return View("Error");
        }
    }

    private List<VideoModel> GetVideoFiles()
    {
        var videoPath = Path.Combine(_webHostEnvironment.WebRootPath, "Media");

        if (!Directory.Exists(videoPath))
        {
            Directory.CreateDirectory(videoPath);
        }

        var videoFiles = Directory.GetFiles(videoPath)
                                   .Select(f => new VideoModel
                                   {
                                       Title = Path.GetFileNameWithoutExtension(f),
                                       FileName = Path.GetFileName(f)
                                   })
                                   .ToList();

        return videoFiles;
    }

    [HttpPost]
    public IActionResult Delete(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Media", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                return RedirectToAction("Index");
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }
}
