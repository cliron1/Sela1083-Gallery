using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;
using GalleyApp.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;

namespace GalleyApp.Controllers;
public class HomeController : Controller {
    private readonly IConfiguration config;
    private readonly ILogger<HomeController> _logger;
	private BlobContainerClient container;

	public HomeController(IConfiguration config, ILogger<HomeController> logger) {
        this.config = config;
        _logger = logger;

		var connectionString = config.GetConnectionString("Storage");

		var client = new BlobServiceClient(connectionString);
		container = client.GetBlobContainerClient("images");
		container.CreateIfNotExists();
	}

    public async Task<IActionResult> Index() {
        var model = new List<string>();
        await foreach(BlobItem blobItem in container.GetBlobsAsync()) {
            model.Add(blobItem.Name);
        }
        return View(model);
    }

    [HttpPost]
    public async Task<ActionResult> Index(IFormFile file) {
        var filePath = Guid.NewGuid().ToString("N") + Path.GetExtension(file.FileName);

		// Get a reference to a blob
		var blob = container.GetBlobClient(filePath);

		// Upload data from the local file
		await blob.UploadAsync(file.OpenReadStream());

		blob.SetHttpHeaders(new BlobHttpHeaders {
			ContentType = file.ContentType
		});

		return RedirectToAction(nameof(Index));
	}

	public IActionResult Privacy() {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error() {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
