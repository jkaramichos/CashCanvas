namespace CashCanvas.Dtos.Plaid;

public class PlaidTransactionDto
{
    public string TransactionId { get; set; } = string.Empty;
    public string AccountId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public DateTime Date { get; set; }
    public string Name { get; set; } = string.Empty;
    public List<string> Category { get; set; } = new();
    public bool Pending { get; set; }
}