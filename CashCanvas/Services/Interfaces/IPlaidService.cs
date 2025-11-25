// Services/IPlaidService.cs
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using CashCanvas.Dtos.Plaid;

namespace CashCanvas.Services.Interfaces;

/// <summary>
/// Handles all API Calls to Plaid. Use the IPlaidUserDataRepository to persist any data retrieved from Plaid.
/// </summary>
public interface IPlaidService
{
    /// <summary>
    /// Creates a link token required to initialize the Plaid Link flow on the client-side.
    /// </summary>
    /// <param name="userId">The internal user ID to associate with the Plaid item.</param>
    /// <returns>A link token string.</returns>
    Task<string> CreateLinkTokenAsync(string userId);

    /// <summary>
    /// Exchanges a temporary public token from Plaid Link for a permanent access token.
    /// </summary>
    /// <param name="publicToken">The public token received from the client-side Link flow.</param>
    /// <returns>The permanent access token for making API calls.</returns>
    Task<string> ExchangePublicTokenAsync(string publicToken);

    /// <summary>
    /// Fetches financial account details using an access token.
    /// </summary>
    /// <param name="accessToken">The access token for the Plaid item.</param>
    /// <returns>A collection of financial accounts.</returns>
    Task<IEnumerable<PlaidAccountDto>> GetAccountsAsync(string accessToken);

    /// <summary>
    /// Fetches transactions for a Plaid item over a specified date range.
    /// </summary>
    /// <param name="accessToken">The access token for the Plaid item.</param>
    /// <param name="startDate">The start date for the transaction query.</param>
    /// <param name="endDate">The end date for the transaction query.</param>
    /// <returns>A collection of transactions.</returns>
    Task<IEnumerable<PlaidTransactionDto>> GetTransactionsAsync(string accessToken, DateTime startDate, DateTime endDate);
}