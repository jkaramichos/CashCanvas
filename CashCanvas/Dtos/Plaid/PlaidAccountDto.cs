
namespace CashCanvas.Dtos.Plaid;

    public class PlaidAccountDto
    {
        public string AccountId { get; set; } = string.Empty;
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty; // e.g., "depository"
        public string Subtype { get; set; } = string.Empty; // e.g., "checking"
        public PlaidBalanceDto Balances { get; set; } = new();
    }