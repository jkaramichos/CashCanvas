// CashCanvas/Services/Implementations/PlaidService.cs
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using CashCanvas.Dtos.Plaid;
using CashCanvas.Services.Interfaces;
using Going.Plaid;
using Going.Plaid.Accounts;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Logging;

namespace CashCanvas.Services.Implementations;

public class PlaidService : IPlaidService
{
    private readonly IPlaidClientAdapter _plaidClientAdapter;
    private readonly ILogger<PlaidService> _logger;

    public PlaidService(IPlaidClientAdapter plaidClientAdapter, ILogger<PlaidService> logger)
    {
        _plaidClientAdapter = plaidClientAdapter;
        _logger = logger;
    }

    public async Task<string> CreateLinkTokenAsync(string userId)
    {
        try
        {
            var request = new LinkTokenCreateRequest
            {
                User = new LinkTokenCreateRequestUser { ClientUserId = userId },
                ClientName = "CashCanvas",
                Products = new[] { Products.Auth, Products.Transactions },
                CountryCodes = new[] { CountryCode.Us },
                Language = Language.English,
            };

            var response = await _plaidClientAdapter.CreateLinkTokenAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Plaid API error while creating link token for user {UserId}: {Error}", userId, response.Error);
                throw new InvalidOperationException($"Plaid API error: {response.Error.ErrorMessage}");
            }

            return response.LinkToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateLinkTokenAsync for user {UserId}", userId);
            throw; // Re-throw to allow UI to handle it
        }
    }

    public async Task<string> ExchangePublicTokenAsync(string publicToken)
    {
        try
        {
            var request = new ItemPublicTokenExchangeRequest { PublicToken = publicToken };
            var response = await _plaidClientAdapter.ExchangePublicTokenAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Plaid API error while exchanging public token: {Error}", response.Error);
                throw new InvalidOperationException($"Plaid API error: {response.Error.ErrorMessage}");
            }

            return response.AccessToken;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in ExchangePublicTokenAsync.");
            throw;
        }
    }

    public async Task<IEnumerable<PlaidAccountDto>> GetAccountsAsync(string accessToken)
    {
        try
        {
            var request = new AccountsGetRequest { AccessToken = accessToken };
            var response = await _plaidClientAdapter.GetAccountsAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Plaid API error while getting accounts: {Error}", response.Error);
                throw new InvalidOperationException($"Plaid API error: {response.Error.ErrorMessage}");
            }

            return response.Accounts.Select(account => new PlaidAccountDto
            {
                AccountId = account.AccountId,
                Name = account.Name,
                Type = account.Type.ToString(),
                Subtype = account.Subtype.ToString(),
                Balances = new PlaidBalanceDto
                {
                    Current = account.Balances.Current ?? 0,
                    Available = account.Balances.Available ?? 0
                }
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAccountsAsync.");
            throw;
        }
    }

    public async Task<IEnumerable<PlaidTransactionDto>> GetTransactionsAsync(string accessToken, DateTime startDate, DateTime endDate)
    {
        try
        {
            var request = new TransactionsGetRequest
            {
                AccessToken = accessToken,
                StartDate = DateOnly.FromDateTime(startDate),
                EndDate = DateOnly.FromDateTime(endDate),
            };
            var response = await _plaidClientAdapter.GetTransactionsAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError("Plaid API error while getting transactions: {Error}", response.Error);
                throw new InvalidOperationException($"Plaid API error: {response.Error.ErrorMessage}");
            }

            return response.Transactions.Select(t => new PlaidTransactionDto
            {
                TransactionId = t.TransactionId,
                AccountId = t.AccountId,
                Amount = t.Amount ?? 0m,
                Date = t.Date?.ToDateTime(TimeOnly.MinValue) ?? DateTime.MinValue,
                Name = t.Name,
                Category = t.Category?.ToList() ?? new List<string>(),
                Pending = t.Pending ?? false
            }).ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetTransactionsAsync.");
            throw;
        }
    }
}
