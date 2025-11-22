using System;
using System.Linq;
using System.Threading.Tasks;
using CashCanvas.Services.Implementations;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Sandbox;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

namespace CashCanvas.Tests.Integration.Services;

[Trait("Category", "Integration")]
public class PlaidServiceIntegrationTests
{
    private readonly PlaidService _plaidService;
    private readonly PlaidClient _plaidClient; // Keep a reference to the raw client

    public PlaidServiceIntegrationTests()
    {
        var config = new ConfigurationBuilder()
            .AddUserSecrets<PlaidServiceIntegrationTests>()
            .Build();

        _plaidClient = new PlaidClient(
            Going.Plaid.Environment.Sandbox,
            clientId: config["Plaid:ClientId"],
            secret: config["Plaid:Secret"]);

        var plaidClientAdapter = new PlaidClientAdapter(_plaidClient);
        _plaidService = new PlaidService(plaidClientAdapter, new NullLogger<PlaidService>());
    }

    [Fact]
    public async Task PlaidApi_FullFlow_ShouldSucceed()
    {
        // Arrange: Use a static user ID for the sandbox
        var userId = "test-user";

        // Act & Assert

        // 1. Create Link Token
        var linkToken = await _plaidService.CreateLinkTokenAsync(userId);
        Assert.NotNull(linkToken);
        Assert.StartsWith("link-sandbox-", linkToken);

        // 2. (Sandbox Only) Create a Public Token for testing
        var sandboxRequest = new SandboxPublicTokenCreateRequest
        {
            InstitutionId = "ins_3", // A common sandbox institution
            InitialProducts = new[] { Products.Transactions }
        };
        var sandboxResponse = await _plaidClient.SandboxPublicTokenCreateAsync(sandboxRequest);
        var publicToken = sandboxResponse.PublicToken;
        Assert.NotNull(publicToken);

        // 3. Exchange Public Token for Access Token
        var accessToken = await _plaidService.ExchangePublicTokenAsync(publicToken);
        Assert.NotNull(accessToken);
        Assert.StartsWith("access-sandbox-", accessToken);

        // 4. Get Accounts
        var accounts = (await _plaidService.GetAccountsAsync(accessToken)).ToList();
        Assert.NotNull(accounts);
        Assert.NotEmpty(accounts);
        Assert.Contains(accounts, a => !string.IsNullOrEmpty(a.Name));

        // 5. (Sandbox Only) Force transactions to be available
        var webhookRequest = new SandboxItemFireWebhookRequest
        {
            AccessToken = accessToken,
            WebhookCode = SandboxItemFireWebhookRequestWebhookCodeEnum.DefaultUpdate
        };
        await _plaidClient.SandboxItemFireWebhookAsync(webhookRequest);

        // Add a delay to allow Plaid to process the transactions
        await Task.Delay(2000); // 2 second delay

        // 6. Get Transactions
        var startDate = DateTime.Now.AddDays(-30);
        var endDate = DateTime.Now;
        var transactions = (await _plaidService.GetTransactionsAsync(accessToken, startDate, endDate)).ToList();
        Assert.NotNull(transactions);
        Assert.NotEmpty(transactions);
    }
}
