using Microsoft.AspNetCore.Mvc;
using VideosMvcApp.Models;

public class HomeController : Controller
{
    private readonly IWebHostEnvironment _webHostEnvironment;
    private readonly ILogger<HomeController> _logger;
    dynamic fileSize = "";

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
                if (file.Length > 0)
                {
                    // Check file extension
                    if (Path.GetExtension(file.FileName).ToLower() != ".mp4")
                    {
                        ViewBag.ErrorMessage = "Only .mp4 files are allowed.";
                        return View("Error");
                    }

                    // Check file size (max 200 MB)
                    if (file.Length > 200 * 1024 * 1024)
                    {
                        ViewBag.ErrorMessage = "File size exceeds the allowed limit (200 MB).";
                        return View("Error");
                    }

                    var fileName = $"{file.FileName}";
                    var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Media", fileName);
                    fileSize = FormatBytes(file.Length);

                    // If the file already exists, replace it
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }

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
                                       FileName = Path.GetFileName(f),
                                       FileSize = Convert.ToString(FormatBytes(new FileInfo(Path.Combine(_webHostEnvironment.WebRootPath, "Media", Path.GetFileName(f))).Length))
                                   })
                                   .ToList();

        return videoFiles;
    }

    [HttpPost]
    public IActionResult DeleteVideo(string fileName)
    {
        try
        {
            var filePath = Path.Combine(_webHostEnvironment.WebRootPath, "Media", fileName);

            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                
                var videoFiles = GetVideoFiles();
                return View("Index", videoFiles);
            }

            return NotFound();
        }
        catch (Exception ex)
        {
            return StatusCode(500, "Internal Server Error");
        }
    }

    // Add this function in your HomeController.cs file
    private string FormatBytes(long bytes, int decimals = 2)
    {
        if (bytes == 0) return "0 Bytes";

        const int k = 1024;
        string[] sizes = { "Bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        int i = (int)Math.Floor(Math.Log(bytes) / Math.Log(k));

        return string.Format("{0} {1}", Math.Round(bytes / Math.Pow(k, i), decimals), sizes[i]);
    }

}
