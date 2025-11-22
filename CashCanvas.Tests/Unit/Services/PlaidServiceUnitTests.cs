using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using CashCanvas.Services.Implementations;
using CashCanvas.Services.Interfaces;
using Going.Plaid;
using Going.Plaid.Accounts;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace CashCanvas.Tests.Unit.Services;

public class PlaidServiceUnitTests
{
    private readonly Mock<IPlaidClientAdapter> _mockPlaidClientAdapter;
    private readonly PlaidService _plaidService;

    public PlaidServiceUnitTests()
    {
        _mockPlaidClientAdapter = new Mock<IPlaidClientAdapter>();
        var mockLogger = new Mock<ILogger<PlaidService>>();
        _plaidService = new PlaidService(_mockPlaidClientAdapter.Object, mockLogger.Object);
    }

    private static TResponse SetResponseStatusCode<TResponse>(TResponse response, HttpStatusCode statusCode) where TResponse : ResponseBase
    {
        var propertyInfo = typeof(ResponseBase).GetProperty("StatusCode", BindingFlags.Public | BindingFlags.Instance);
        if (propertyInfo != null && propertyInfo.CanWrite)
        {
            propertyInfo.SetValue(response, statusCode, null);
        }
        else // Fallback to reflection on the backing field if property is not writable
        {
            var fieldInfo = typeof(ResponseBase).GetField("<StatusCode>k__BackingField", BindingFlags.Instance | BindingFlags.NonPublic);
            fieldInfo?.SetValue(response, statusCode);
        }
        return response;
    }

    #region CreateLinkTokenAsync Tests

    [Fact]
    public async Task CreateLinkTokenAsync_WhenApiCallIsSuccessful_ReturnsLinkToken()
    {
        // Arrange
        var expectedToken = "test-link-token";
        var userId = "test-user";
        var response = new LinkTokenCreateResponse { LinkToken = expectedToken };
        var successResponse = SetResponseStatusCode(response, HttpStatusCode.OK);

        _mockPlaidClientAdapter
            .Setup(c => c.CreateLinkTokenAsync(It.IsAny<LinkTokenCreateRequest>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _plaidService.CreateLinkTokenAsync(userId);

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public async Task CreateLinkTokenAsync_WhenApiReturnsError_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new LinkTokenCreateResponse { Error = new PlaidError { ErrorMessage = "Invalid API key" } };
        var errorResponse = SetResponseStatusCode(response, HttpStatusCode.Unauthorized);

        _mockPlaidClientAdapter
            .Setup(c => c.CreateLinkTokenAsync(It.IsAny<LinkTokenCreateRequest>()))
            .ReturnsAsync(errorResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() => _plaidService.CreateLinkTokenAsync("test-user"));
        Assert.Contains("Plaid API error", exception.Message);
    }

    [Fact]
    public async Task CreateLinkTokenAsync_WhenClientThrowsException_ThrowsSameException()
    {
        // Arrange
        _mockPlaidClientAdapter
            .Setup(c => c.CreateLinkTokenAsync(It.IsAny<LinkTokenCreateRequest>()))
            .ThrowsAsync(new HttpRequestException("Network error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => _plaidService.CreateLinkTokenAsync("test-user"));
    }

    #endregion

    #region ExchangePublicTokenAsync Tests

    [Fact]
    public async Task ExchangePublicTokenAsync_WhenApiCallIsSuccessful_ReturnsAccessToken()
    {
        // Arrange
        var expectedToken = "test-access-token";
        var response = new ItemPublicTokenExchangeResponse { AccessToken = expectedToken };
        var successResponse = SetResponseStatusCode(response, HttpStatusCode.OK);

        _mockPlaidClientAdapter
            .Setup(c => c.ExchangePublicTokenAsync(It.IsAny<ItemPublicTokenExchangeRequest>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _plaidService.ExchangePublicTokenAsync("public-token");

        // Assert
        Assert.Equal(expectedToken, result);
    }

    [Fact]
    public async Task ExchangePublicTokenAsync_WhenApiReturnsError_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new ItemPublicTokenExchangeResponse { Error = new PlaidError { ErrorMessage = "Invalid public token" } };
        var errorResponse = SetResponseStatusCode(response, HttpStatusCode.BadRequest);

        _mockPlaidClientAdapter
            .Setup(c => c.ExchangePublicTokenAsync(It.IsAny<ItemPublicTokenExchangeRequest>()))
            .ReturnsAsync(errorResponse);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _plaidService.ExchangePublicTokenAsync("public-token"));
    }

    #endregion

    #region GetAccountsAsync Tests

    [Fact]
    public async Task GetAccountsAsync_WhenApiCallIsSuccessful_ReturnsAccountDtos()
    {
        // Arrange
        var mockBalances = new AccountBalance { Current = 100m, Available = 90m };
        var mockAccount = new Account
        {
            AccountId = "1",
            Name = "Checking",
            Type = AccountType.Depository,
            Subtype = AccountSubtype.Checking,
            Balances = mockBalances
        };
        var response = new AccountsGetResponse { Accounts = new List<Account> { mockAccount } };
        var successResponse = SetResponseStatusCode(response, HttpStatusCode.OK);

        _mockPlaidClientAdapter
            .Setup(c => c.GetAccountsAsync(It.IsAny<AccountsGetRequest>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _plaidService.GetAccountsAsync("access-token");

        // Assert
        var account = Assert.Single(result);
        Assert.Equal("1", account.AccountId);
        Assert.Equal("Checking", account.Name);
        Assert.Equal(100m, account.Balances.Current);
    }

    [Fact]
    public async Task GetAccountsAsync_WhenApiReturnsError_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new AccountsGetResponse { Error = new PlaidError { ErrorMessage = "Invalid access token" } };
        var errorResponse = SetResponseStatusCode(response, HttpStatusCode.Unauthorized);

        _mockPlaidClientAdapter
            .Setup(c => c.GetAccountsAsync(It.IsAny<AccountsGetRequest>()))
            .ReturnsAsync(errorResponse);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _plaidService.GetAccountsAsync("access-token"));
    }

    #endregion

    #region GetTransactionsAsync Tests

    [Fact]
    public async Task GetTransactionsAsync_WhenApiCallIsSuccessful_ReturnsTransactionDtos()
    {
        // Arrange
        var mockTransaction = new Transaction
        {
            TransactionId = "1",
            AccountId = "acc-1",
            Amount = 50.25m,
            Date = new DateOnly(2023, 1, 1),
            Name = "Coffee Shop"
        };
        var response = new TransactionsGetResponse { Transactions = new List<Transaction> { mockTransaction } };
        var successResponse = SetResponseStatusCode(response, HttpStatusCode.OK);

        _mockPlaidClientAdapter
            .Setup(c => c.GetTransactionsAsync(It.IsAny<TransactionsGetRequest>()))
            .ReturnsAsync(successResponse);

        // Act
        var result = await _plaidService.GetTransactionsAsync("access-token", DateTime.Now, DateTime.Now);

        // Assert
        var transaction = Assert.Single(result);
        Assert.Equal("1", transaction.TransactionId);
        Assert.Equal(50.25m, transaction.Amount);
    }

    [Fact]
    public async Task GetTransactionsAsync_WhenApiReturnsError_ThrowsInvalidOperationException()
    {
        // Arrange
        var response = new TransactionsGetResponse { Error = new PlaidError { ErrorMessage = "Invalid date range" } };
        var errorResponse = SetResponseStatusCode(response, HttpStatusCode.BadRequest);

        _mockPlaidClientAdapter
            .Setup(c => c.GetTransactionsAsync(It.IsAny<TransactionsGetRequest>()))
            .ReturnsAsync(errorResponse);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _plaidService.GetTransactionsAsync("access-token", DateTime.Now, DateTime.Now));
    }

    #endregion
}
