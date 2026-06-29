using RossWright.MetalInjection;
using Shouldly;
using Xunit;

namespace RossWright.MetalInjection.Abstractions.UnitTests;

public class ConfigSectionAttributeTests
{
    [Fact]
    public void Constructor_WithSectionTitle_SetsSectionTitleProperty()
    {
        // Arrange
        const string sectionTitle = "TestSection";

        // Act
        var attribute = new ConfigSectionAttribute(sectionTitle);

        // Assert
        attribute.SectionTitle.ShouldBe(sectionTitle);
    }

    [Fact]
    public void Constructor_WithSectionTitle_SetsRegisterAsToNull()
    {
        // Arrange
        const string sectionTitle = "TestSection";

        // Act
        var attribute = new ConfigSectionAttribute(sectionTitle);

        // Assert
        attribute.RegisterAs.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithSectionTitleAndRegisterAs_SetsBothProperties()
    {
        // Arrange
        const string sectionTitle = "TestSection";
        var registerAs = typeof(ITestInterface);

        // Act
        var attribute = new ConfigSectionAttribute(sectionTitle, registerAs);

        // Assert
        attribute.SectionTitle.ShouldBe(sectionTitle);
        attribute.RegisterAs.ShouldBe(registerAs);
    }

    [Fact]
    public void Constructor_WithSectionTitleAndNullRegisterAs_SetsRegisterAsToNull()
    {
        // Arrange
        const string sectionTitle = "TestSection";

        // Act
        var attribute = new ConfigSectionAttribute(sectionTitle, null);

        // Assert
        attribute.SectionTitle.ShouldBe(sectionTitle);
        attribute.RegisterAs.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyString_SetsEmptyString()
    {
        // Arrange
        const string sectionTitle = "";

        // Act
        var attribute = new ConfigSectionAttribute(sectionTitle);

        // Assert
        attribute.SectionTitle.ShouldBe(sectionTitle);
    }

    [Fact]
    public void GenericConstructor_WithSectionTitle_SetsSectionTitleProperty()
    {
        // Arrange
        const string sectionTitle = "TestSection";

        // Act
        var attribute = new ConfigSectionAttribute<ITestInterface>(sectionTitle);

        // Assert
        attribute.SectionTitle.ShouldBe(sectionTitle);
    }

    [Fact]
    public void GenericConstructor_WithSectionTitle_SetsRegisterAsToGenericType()
    {
        // Arrange
        const string sectionTitle = "TestSection";

        // Act
        var attribute = new ConfigSectionAttribute<ITestInterface>(sectionTitle);

        // Assert
        attribute.RegisterAs.ShouldBe(typeof(ITestInterface));
    }

    [Fact]
    public void GenericConstructor_WithDifferentType_SetsRegisterAsToThatType()
    {
        // Arrange
        const string sectionTitle = "TestSection";

        // Act
        var attribute = new ConfigSectionAttribute<string>(sectionTitle);

        // Assert
        attribute.RegisterAs.ShouldBe(typeof(string));
    }

    [Fact]
    public void GenericConstructor_WithEmptyString_SetsEmptyString()
    {
        // Arrange
        const string sectionTitle = "";

        // Act
        var attribute = new ConfigSectionAttribute<ITestInterface>(sectionTitle);

        // Assert
        attribute.SectionTitle.ShouldBe(sectionTitle);
    }

    private interface ITestInterface
    {
    }
}
