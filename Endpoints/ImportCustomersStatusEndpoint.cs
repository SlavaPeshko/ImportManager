using ImportManager.Data;
using Microsoft.EntityFrameworkCore;

namespace ImportManager.Endpoints;

public static class ImportCustomersStatusEndpoint
{
    public static IEndpointRouteBuilder MapImportCustomersStatus(this IEndpointRouteBuilder endpoints)
    {
        endpoints.Map("/import/{id:guid}", async (Guid id, ImportManagerContext context) =>
            {
                var importJob = await context.ImportJobs.FirstOrDefaultAsync(x => x.DomainId == id);
                if (importJob == null)
                {
                    return Results.NotFound();
                }

                return Results.Ok(new
                {
                    id = importJob.DomainId,
                    status = importJob.Status.ToString(),
                    createdAt = importJob.CreatedAt,
                    updatedAt = importJob.StartedAt ?? importJob.FailedAt ?? importJob.CompletedAt,
                    notes = importJob.FailureReason
                });
            });

        return endpoints;
    }
}