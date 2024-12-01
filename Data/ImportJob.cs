namespace ImportManager.Data;

public class ImportJob
{
    public int Id { get; set; }
    public Guid DomainId { get; set; }
    public required string FileName { get; set; }
    public JobStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public DateTime? FailedAt { get; set; }
    public string FailureReason { get; set; }
}