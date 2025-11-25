using Bunit;
using CashCanvas.Components.Pages;
using CashCanvas.Data;
using CashCanvas.Entities;
using CashCanvas.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using Xunit;
using MudBlazor.Services;
using System.Threading.Tasks;
using CashCanvas.Dtos;
using CashCanvas.Services.Interfaces; // Add this using for Task

namespace CashCanvas.Tests.Unit.Components;

public class CounterUnitTests : BunitContext
{
    public CounterUnitTests()
    {
        // Register MudBlazor services required for its components to render
        Services.AddMudServices();
    }

    [Fact]
    public async Task Counter_RendersAndInitializes_WithAuthenticatedUser()
    {
        var userId = "test-user-id";

        var mockStatsService = new Mock<IUserStatsService>();
        mockStatsService.Setup(s => s.GetStatsAsync(userId))
            .ReturnsAsync(() => new UserStatsDto { UserId = userId, TotalCounterClicks = 5 });

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);
        mockUserManager.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .Returns(userId);

        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("TEST USER");
        authContext.SetClaims(new System.Security.Claims.Claim("sub", userId));

        Services.AddSingleton(mockStatsService.Object);
        Services.AddSingleton(mockUserManager.Object);

        var cut = Render<Counter>();

        await cut.WaitForStateAsync(() => cut.FindAll("p.mud-typography").Count > 0);

        cut.Find("h1").MarkupMatches("<h1>Counter</h1>");
        cut.Find("p.mud-typography")
            .MarkupMatches(@"<p class=""mud-typography mud-typography-body1"">Current Count: 5</p>");

        mockStatsService.Verify(s => s.GetStatsAsync(userId), Times.Once);
    }


    [Fact]
    public async Task ClickingIncrementButton_UpdatesCountAndCallsService()
    {
        // Arrange
        var userId = "test-user-id";
        UserStatsDto? capturedStats = null;

        var mockStatsService = new Mock<IUserStatsService>();
        // Return a new object each time to prevent mutation issues
        mockStatsService.Setup(s => s.GetStatsAsync(userId))
            .ReturnsAsync(() => new UserStatsDto { UserId = userId, TotalCounterClicks = 5 });

        mockStatsService.Setup(s => s.UpdateStatsAsync(It.IsAny<UserStatsDto>()))
            .Callback<UserStatsDto>(stats => capturedStats = stats)
            .Returns(Task.CompletedTask);

        var mockUserManager = new Mock<UserManager<ApplicationUser>>(
            Mock.Of<IUserStore<ApplicationUser>>(), null, null, null, null, null, null, null, null);


        mockUserManager.Setup(m => m.GetUserId(It.IsAny<System.Security.Claims.ClaimsPrincipal>()))
            .Returns(userId);

        var authContext = this.AddAuthorization();
        authContext.SetAuthorized("TEST USER");
        authContext.SetClaims(new System.Security.Claims.Claim("sub", userId));

        Services.AddSingleton(mockStatsService.Object);
        Services.AddSingleton(mockUserManager.Object);

        var cut = Render<Counter>();

        // Wait for the initial render and data load to complete
        await cut.WaitForStateAsync(() => cut.FindAll("p.mud-typography").Count > 0);
        // `CashCanvas.Tests/Unit/Components/CounterTests.cs`
        cut.Find("p.mud-typography")
            .MarkupMatches(@"<p class=""mud-typography mud-typography-body1"">Current Count: 5</p>");


        // Act: Find the button and click it, awaiting the async event handler
        await cut.Find("button").ClickAsync(new Microsoft.AspNetCore.Components.Web.MouseEventArgs());

        // Assert: The UI should have updated. A short wait can help if there are multiple renders.
        // CashCanvas.Tests/Unit/Components/CounterTests.cs
        cut.WaitForAssertion(() =>
            cut.Find("p.mud-typography")
                .MarkupMatches(@"<p class=""mud-typography mud-typography-body1"">Current Count: 6</p>"));


        // Verify the service call and the captured object
        mockStatsService.Verify(s => s.UpdateStatsAsync(It.IsAny<UserStatsDto>()), Times.Once);
        Assert.NotNull(capturedStats);
        Assert.Equal(6, capturedStats.TotalCounterClicks);
    }

}
