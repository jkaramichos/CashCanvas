using Going.Plaid.Accounts;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Going.Plaid.Transactions;

namespace CashCanvas.Services.Interfaces;

public interface IPlaidClientAdapter
{
    Task<LinkTokenCreateResponse> CreateLinkTokenAsync(LinkTokenCreateRequest request);
    Task<ItemPublicTokenExchangeResponse> ExchangePublicTokenAsync(ItemPublicTokenExchangeRequest request);
    Task<AccountsGetResponse> GetAccountsAsync(AccountsGetRequest request);
    Task<TransactionsGetResponse> GetTransactionsAsync(TransactionsGetRequest request);
}