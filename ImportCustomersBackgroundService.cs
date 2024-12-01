using ImportManager.Data;
using Microsoft.EntityFrameworkCore;

namespace ImportManager;

public class ImportCustomersBackgroundService(IServiceScopeFactory scopeFactory) : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory =
        scopeFactory ?? throw new ArgumentNullException(nameof(scopeFactory));

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using var timer = new PeriodicTimer(TimeSpan.FromSeconds(5));
        while (!stoppingToken.IsCancellationRequested && await timer.WaitForNextTickAsync(stoppingToken))
        {
            using var scope = _scopeFactory.CreateScope();
            var fileUploadManagerContext = scope.ServiceProvider.GetRequiredService<ImportManagerContext>();
            var jobs = await fileUploadManagerContext.ImportJobs.Where(x => x.Status == JobStatus.Enqueued)
                .ToListAsync(cancellationToken: stoppingToken);

            foreach (var job in jobs)
            {
                job.Status = JobStatus.Running;
                job.StartedAt = DateTime.UtcNow;
                await fileUploadManagerContext.SaveChangesAsync(stoppingToken);

                try
                {
                    await UploadFileAsync(job, fileUploadManagerContext, stoppingToken);
                    job.Status = JobStatus.Completed;
                    job.CompletedAt = DateTime.UtcNow;
                }
                catch (Exception exception)
                {
                    job.Status = JobStatus.Failed;
                    job.FailedAt = DateTime.UtcNow;
                    job.FailureReason = exception.Message;
                }
                finally
                {
                    await fileUploadManagerContext.SaveChangesAsync(stoppingToken);
                }
            }
        }
    }

    private static async Task UploadFileAsync(ImportJob job, ImportManagerContext importManagerContext, CancellationToken cancellationToken)
    {
        var path = Path.Combine("files", job.FileName);
        using var reader = new StreamReader(path);

        _ = await reader.ReadLineAsync(cancellationToken);
        List<Customer> customers = [];

        while (await reader.ReadLineAsync(cancellationToken) is { } customer)
        {
            var customerValues = SplitCsvLine(customer);
            customers.Add(new Customer
            {
                FirstName = customerValues[2],
                LastName = customerValues[3],
                Company = customerValues[4],
                City = customerValues[5],
                Country = customerValues[6],
                Phone = customerValues[7],
                Email = customerValues[9],
                SubscriptionDate = DateTime.TryParse(customerValues[10], out var subscriptionDate) ? subscriptionDate : null,
                Website = customerValues[11]
            });

            if (customers.Count % 10 != 0)
            {
                continue;
            }

            await importManagerContext.Customers.AddRangeAsync(customers, cancellationToken);
            await importManagerContext.SaveChangesAsync(cancellationToken);

            customers.Clear();
        }

        await importManagerContext.Customers.AddRangeAsync(customers, cancellationToken);
        await importManagerContext.SaveChangesAsync(cancellationToken);
    }

    private static string[] SplitCsvLine(string line)
    {
        List<string> result = [];
        var inQuote = false;
        var startIndex = 0;

        for (int i = 0; i < line.Length; i++)
        {
            if (line[i] == '"')
            {
                inQuote = !inQuote;
            }
            else if (line[i] == ',' && !inQuote)
            {
                result.Add(line.Substring(startIndex, i - startIndex).Trim('"'));
                startIndex = i + 1;
            }
        }

        if (startIndex < line.Length)
        {
            result.Add(line.Substring(startIndex).Trim('"'));
        }

        return result.ToArray();
    }
}