using CashCanvas.Services.Interfaces;
using Going.Plaid;
using Going.Plaid.Accounts;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Going.Plaid.Transactions;

namespace CashCanvas.Services.Implementations;

public class PlaidClientAdapter : IPlaidClientAdapter
{
    private readonly PlaidClient _plaidClient;

    public PlaidClientAdapter(PlaidClient plaidClient)
    {
        _plaidClient = plaidClient;
    }

    public Task<LinkTokenCreateResponse> CreateLinkTokenAsync(LinkTokenCreateRequest request) =>
        _plaidClient.LinkTokenCreateAsync(request);

    public Task<ItemPublicTokenExchangeResponse> ExchangePublicTokenAsync(ItemPublicTokenExchangeRequest request) =>
        _plaidClient.ItemPublicTokenExchangeAsync(request);

    public Task<AccountsGetResponse> GetAccountsAsync(AccountsGetRequest request) =>
        _plaidClient.AccountsGetAsync(request);

    public Task<TransactionsGetResponse> GetTransactionsAsync(TransactionsGetRequest request) =>
        _plaidClient.TransactionsGetAsync(request);
}