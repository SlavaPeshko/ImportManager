namespace ImportManager.Data;

public class Customer
{
    public int Id { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string Company{ get; set; }
    public string City{ get; set; }
    public string Country{ get; set; }
    public string Phone{ get; set; }
    public string Email{ get; set; }
    public DateTime? SubscriptionDate { get; set; }
    public string Website { get; set; }
}