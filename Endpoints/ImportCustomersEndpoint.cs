using ImportManager.Data;

namespace ImportManager.Endpoints;

public static class ImportCustomersEndpoint
{
    private const string CsvFileExtension = ".csv";

    public static IEndpointRouteBuilder MapImportCustomers(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/import", async (IFormFile file, ImportManagerContext context) =>
        {
            if (file.Length == 0 || !file.FileName.EndsWith(CsvFileExtension, StringComparison.OrdinalIgnoreCase))
            {
                return Results.BadRequest("Invalid file. Please provide non-empty file.");
            }

            var jobId = Guid.NewGuid();
            await SaveFileToTempFolderAsync(file, jobId);

            var importJob = new ImportJob
            {
                FileName = $"{jobId}{CsvFileExtension}",
                DomainId = jobId,
                Status = JobStatus.Enqueued,
                CreatedAt = DateTime.UtcNow,
            };

            await context.ImportJobs.AddAsync(importJob);
            await context.SaveChangesAsync();

            return Results.Ok(new { id = jobId, message = "File uploaded successfully. Import is starting..." });
        })
            .DisableAntiforgery();

        return endpoints;
    }

    private static async Task SaveFileToTempFolderAsync(IFormFile file, Guid jobId)
    {
        if (!Directory.Exists("files"))
        {
            Directory.CreateDirectory("files");
        }

        var filePath = Path.Combine("files", $"{jobId}{CsvFileExtension}");
        await using var stream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
        await file.OpenReadStream().CopyToAsync(stream);
    }
}