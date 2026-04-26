using RossWright.MetalCommand.Tests.Infrastructure;

namespace RossWright.MetalCommand.Tests;

public class TryParseEnvironmentExtensionTests
{
    [Fact]
    public void TryParseEnvironment_ValidEnvironment_ReturnsEnvironmentName()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var entry = new EnvironmentEntry { Name = "Dev", IsProtected = false };
        source.Environments.Returns([entry]);

        // Act
        var result = console.TryParseEnvironment(source, "dev");

        // Assert
        result.ShouldBe("Dev");
    }

    [Fact]
    public void TryParseEnvironment_UnknownEnvironment_WithValidAlternatives_ReturnsNullAndWritesError()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var dev = new EnvironmentEntry { Name = "Dev", IsProtected = false };
        var staging = new EnvironmentEntry { Name = "Staging", IsProtected = false };
        source.Environments.Returns([dev, staging]);

        // Act
        var result = console.TryParseEnvironment(source, "InvalidEnv");

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("Unknown environment");
        console.ErrorLines[0].ShouldContain("Dev");
        console.ErrorLines[0].ShouldContain("Staging");
    }

    [Fact]
    public void TryParseEnvironment_UnknownEnvironment_WithNoValidEnvironments_ReturnsNullAndWritesError()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        source.Environments.Returns([]);

        // Act
        var result = console.TryParseEnvironment(source, "InvalidEnv");

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("Unknown environment");
        console.ErrorLines[0].ShouldContain("no valid environments are available for this command");
    }

    [Fact]
    public void TryParseEnvironment_ProtectedEnvironment_ForbiddenPolicy_ReturnsNullAndWritesError()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        var dev = new EnvironmentEntry { Name = "Dev", IsProtected = false };
        source.Environments.Returns([prod, dev]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", EnvironmentPolicy.Forbidden);

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("That environment cannot be used with this command");
        console.ErrorLines[0].ShouldContain("Dev");
    }

    [Fact]
    public void TryParseEnvironment_ProtectedEnvironment_ForbiddenPolicy_NoAlternatives_ReturnsNullAndWritesError()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        source.Environments.Returns([prod]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", EnvironmentPolicy.Forbidden);

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("That environment cannot be used with this command");
        console.ErrorLines[0].ShouldContain("no valid environments are available for this command");
    }

    [Fact]
    public void TryParseEnvironment_ProtectedEnvironment_DangerousPolicy_UserConfirmsYes_ReturnsEnvironmentName()
    {
        // Arrange
        var console = new TestConsole("yes");
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        source.Environments.Returns([prod]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", EnvironmentPolicy.Dangerous);

        // Assert
        result.ShouldBe("Production");
        console.Lines.ShouldContain(l => l.Contains("Are you sure?"));
        console.Lines.ShouldContain(l => l.Contains("type \"yes\" to confirm"));
    }

    [Fact]
    public void TryParseEnvironment_ProtectedEnvironment_DangerousPolicy_UserDeclinesWithNo_ReturnsNull()
    {
        // Arrange
        var console = new TestConsole("no");
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        source.Environments.Returns([prod]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", EnvironmentPolicy.Dangerous);

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldBe("Command aborted");
    }

    [Fact]
    public void TryParseEnvironment_ProtectedEnvironment_DangerousPolicy_UserDeclinesWithEmptyString_ReturnsNull()
    {
        // Arrange
        var console = new TestConsole("");
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        source.Environments.Returns([prod]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", EnvironmentPolicy.Dangerous);

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldBe("Command aborted");
    }

    [Fact]
    public void TryParseEnvironment_ProtectedEnvironment_BenignPolicy_NoPrompt_ReturnsEnvironmentName()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        source.Environments.Returns([prod]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", EnvironmentPolicy.Benign);

        // Assert
        result.ShouldBe("Production");
        console.Lines.ShouldNotContain(l => l.Contains("Are you sure?"));
    }

    [Fact]
    public void TryParseEnvironment_BoolOverload_AllowProtectedTrue_MapsToTransient()
    {
        // Arrange
        var console = new TestConsole("yes");
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        source.Environments.Returns([prod]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", allowProtected: true);

        // Assert
        result.ShouldBe("Production");
        console.Lines.ShouldContain(l => l.Contains("Are you sure?"));
    }

    [Fact]
    public void TryParseEnvironment_BoolOverload_AllowProtectedFalse_MapsToForbidden()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        var dev = new EnvironmentEntry { Name = "Dev", IsProtected = false };
        source.Environments.Returns([prod, dev]);

        // Act
        var result = console.TryParseEnvironment(source, "Production", allowProtected: false);

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.ShouldContain(l => l.Contains("That environment cannot be used with this command"));
    }

    [Fact]
    public void TryParseEnvironment_NullEnvironment_UsesDefaultEnvironment()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var dev = new EnvironmentEntry { Name = "Dev", IsProtected = false };
        source.DefaultEnvironment.Returns("Dev");
        source.Environments.Returns([dev]);

        // Act
        var result = console.TryParseEnvironment(source, null);

        // Assert
        result.ShouldBe("Dev");
    }

    [Fact]
    public void TryParseEnvironment_UnknownEnvironment_ForbiddenPolicy_FiltersProtectedFromSuggestions()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        var staging = new EnvironmentEntry { Name = "Staging", IsProtected = true };
        var dev = new EnvironmentEntry { Name = "Dev", IsProtected = false };
        source.Environments.Returns([prod, staging, dev]);

        // Act
        var result = console.TryParseEnvironment(source, "InvalidEnv", EnvironmentPolicy.Forbidden);

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("Unknown environment");
        console.ErrorLines[0].ShouldContain("Dev");
        console.ErrorLines[0].ShouldNotContain("Production");
        console.ErrorLines[0].ShouldNotContain("Staging");
    }

    [Fact]
    public void TryParseEnvironment_UnknownEnvironment_ForbiddenPolicy_AllProtected_NoSuggestions()
    {
        // Arrange
        var console = new TestConsole();
        var source = Substitute.For<IEnvironmentSource>();
        var prod = new EnvironmentEntry { Name = "Production", IsProtected = true };
        var staging = new EnvironmentEntry { Name = "Staging", IsProtected = true };
        source.Environments.Returns([prod, staging]);

        // Act
        var result = console.TryParseEnvironment(source, "InvalidEnv", EnvironmentPolicy.Forbidden);

        // Assert
        result.ShouldBeNull();
        console.ErrorLines.Count.ShouldBe(1);
        console.ErrorLines[0].ShouldContain("Unknown environment");
        console.ErrorLines[0].ShouldContain("no valid environments are available for this command");
    }
}
