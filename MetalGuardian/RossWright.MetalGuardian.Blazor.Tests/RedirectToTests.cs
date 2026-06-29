using Microsoft.AspNetCore.Components;

namespace RossWright.MetalGuardian.Blazor.Tests;

public class RedirectToTests
{
    [Fact]
    public void OnInitialized_NavigatesToUrl()
    {
        // Arrange
        var navigationManager = new TestNavigationManager();
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var component = new RedirectTo
        {
            NavigationManager = navigationManager,
            Url = "/test-url"
        };
#pragma warning restore BL0005

        // Act
        InvokeOnInitialized(component);

        // Assert
        navigationManager.NavigateToCallCount.ShouldBe(1);
        navigationManager.LastNavigatedUri.ShouldBe("/test-url");
        navigationManager.LastForceLoad.ShouldBe(true);
    }

    [Fact]
    public void OnInitialized_NavigatesToUrlWithForceLoad()
    {
        // Arrange
        var navigationManager = new TestNavigationManager();
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var component = new RedirectTo
        {
            NavigationManager = navigationManager,
            Url = "/another-page"
        };
#pragma warning restore BL0005

        // Act
        InvokeOnInitialized(component);

        // Assert
        navigationManager.LastForceLoad.ShouldBe(true);
    }

    [Fact]
    public void OnInitialized_NavigatesWithAbsoluteUrl()
    {
        // Arrange
        var navigationManager = new TestNavigationManager();
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var component = new RedirectTo
        {
            NavigationManager = navigationManager,
            Url = "https://example.com/page"
        };
#pragma warning restore BL0005

        // Act
        InvokeOnInitialized(component);

        // Assert
        navigationManager.NavigateToCallCount.ShouldBe(1);
        navigationManager.LastNavigatedUri.ShouldBe("https://example.com/page");
        navigationManager.LastForceLoad.ShouldBe(true);
    }

    [Fact]
    public void OnInitialized_NavigatesWithEmptyUrl()
    {
        // Arrange
        var navigationManager = new TestNavigationManager();
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var component = new RedirectTo
        {
            NavigationManager = navigationManager,
            Url = string.Empty
        };
#pragma warning restore BL0005

        // Act
        InvokeOnInitialized(component);

        // Assert
        navigationManager.NavigateToCallCount.ShouldBe(1);
        navigationManager.LastNavigatedUri.ShouldBe(string.Empty);
    }

    [Fact]
    public void OnInitialized_NavigatesWithUrlContainingQueryString()
    {
        // Arrange
        var navigationManager = new TestNavigationManager();
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var component = new RedirectTo
        {
            NavigationManager = navigationManager,
            Url = "/page?param1=value1&param2=value2"
        };
#pragma warning restore BL0005

        // Act
        InvokeOnInitialized(component);

        // Assert
        navigationManager.NavigateToCallCount.ShouldBe(1);
        navigationManager.LastNavigatedUri.ShouldBe("/page?param1=value1&param2=value2");
        navigationManager.LastForceLoad.ShouldBe(true);
    }

    [Fact]
    public void OnInitialized_NavigatesWithUrlContainingFragment()
    {
        // Arrange
        var navigationManager = new TestNavigationManager();
#pragma warning disable BL0005 // Component parameter should not be set outside of its component
        var component = new RedirectTo
        {
            NavigationManager = navigationManager,
            Url = "/page#section"
        };
#pragma warning restore BL0005

        // Act
        InvokeOnInitialized(component);

        // Assert
        navigationManager.NavigateToCallCount.ShouldBe(1);
        navigationManager.LastNavigatedUri.ShouldBe("/page#section");
        navigationManager.LastForceLoad.ShouldBe(true);
    }

    private static void InvokeOnInitialized(RedirectTo component)
    {
        var method = typeof(RedirectTo).GetMethod("OnInitialized",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        method?.Invoke(component, null);
    }

    private sealed class TestNavigationManager : NavigationManager
    {
        public int NavigateToCallCount { get; private set; }
        public string? LastNavigatedUri { get; private set; }
        public bool? LastForceLoad { get; private set; }

        public TestNavigationManager()
        {
            Initialize("https://localhost/", "https://localhost/");
        }

        protected override void NavigateToCore(string uri, bool forceLoad)
        {
            NavigateToCallCount++;
            LastNavigatedUri = uri;
            LastForceLoad = forceLoad;
        }
    }
}
