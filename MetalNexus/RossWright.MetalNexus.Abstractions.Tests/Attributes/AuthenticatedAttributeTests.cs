using RossWright.MetalNexus;
using Shouldly;

namespace RossWright.MetalNexus.Abstractions.UnitTests.Attributes;

public class AuthenticatedAttributeTests
{
    [Fact]
    public void Constructor_WithNoArguments_SetsAuthorizedRolesToNull()
    {
        var attribute = new AuthenticatedAttribute();
        attribute.AuthorizedRoles.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithEmptyArray_SetsAuthorizedRolesToNull()
    {
        var attribute = new AuthenticatedAttribute([]);
        attribute.AuthorizedRoles.ShouldBeNull();
    }

    [Fact]
    public void Constructor_WithSingleStringRole_SetsAuthorizedRoles()
    {
        var attribute = new AuthenticatedAttribute("Admin");
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(1);
        attribute.AuthorizedRoles[0].ShouldBe("Admin");
    }

    [Fact]
    public void Constructor_WithMultipleStringRoles_SetsAuthorizedRoles()
    {
        var attribute = new AuthenticatedAttribute("Admin", "User", "Manager");
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(3);
        attribute.AuthorizedRoles[0].ShouldBe("Admin");
        attribute.AuthorizedRoles[1].ShouldBe("User");
        attribute.AuthorizedRoles[2].ShouldBe("Manager");
    }

    [Fact]
    public void Constructor_WithIntegerRoles_ConvertsToString()
    {
        var attribute = new AuthenticatedAttribute(1, 2, 3);
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(3);
        attribute.AuthorizedRoles[0].ShouldBe("1");
        attribute.AuthorizedRoles[1].ShouldBe("2");
        attribute.AuthorizedRoles[2].ShouldBe("3");
    }

    [Fact]
    public void Constructor_WithMixedTypes_ConvertsAllToString()
    {
        var attribute = new AuthenticatedAttribute("Admin", 42, true, 3.14);
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(4);
        attribute.AuthorizedRoles[0].ShouldBe("Admin");
        attribute.AuthorizedRoles[1].ShouldBe("42");
        attribute.AuthorizedRoles[2].ShouldBe("True");
        attribute.AuthorizedRoles[3].ShouldBe("3.14");
    }

    [Fact]
    public void Constructor_WithZero_ConvertsToString()
    {
        var attribute = new AuthenticatedAttribute(0);
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(1);
        attribute.AuthorizedRoles[0].ShouldBe("0");
    }

    [Fact]
    public void Constructor_WithNegativeValue_ConvertsToString()
    {
        var attribute = new AuthenticatedAttribute(-1);
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(1);
        attribute.AuthorizedRoles[0].ShouldBe("-1");
    }

    [Fact]
    public void Constructor_WithMaxInt_ConvertsToString()
    {
        var attribute = new AuthenticatedAttribute(int.MaxValue);
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(1);
        attribute.AuthorizedRoles[0].ShouldBe("2147483647");
    }

    [Fact]
    public void Constructor_WithMinInt_ConvertsToString()
    {
        var attribute = new AuthenticatedAttribute(int.MinValue);
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(1);
        attribute.AuthorizedRoles[0].ShouldBe("-2147483648");
    }

    [Fact]
    public void Constructor_WithEmptyString_SetsAuthorizedRoles()
    {
        var attribute = new AuthenticatedAttribute("");
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles.Length.ShouldBe(1);
        attribute.AuthorizedRoles[0].ShouldBe("");
    }

    [Fact]
    public void Attribute_CanBeAppliedToClass()
    {
        var attribute = typeof(TestClassWithAttribute)
            .GetCustomAttributes(typeof(AuthenticatedAttribute), false)
            .FirstOrDefault();
        attribute.ShouldNotBeNull();
        attribute.ShouldBeOfType<AuthenticatedAttribute>();
    }

    [Fact]
    public void AttributeUsage_ValidOn_ShouldBeClass()
    {
        var attributeUsage = typeof(AuthenticatedAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;
        attributeUsage.ShouldNotBeNull();
        attributeUsage.ValidOn.ShouldBe(AttributeTargets.Class);
    }

    [Fact]
    public void AttributeUsage_AllowMultiple_ShouldBeFalse()
    {
        var attributeUsage = typeof(AuthenticatedAttribute)
            .GetCustomAttributes(typeof(AttributeUsageAttribute), false)
            .FirstOrDefault() as AttributeUsageAttribute;
        attributeUsage.ShouldNotBeNull();
        attributeUsage.AllowMultiple.ShouldBeFalse();
    }

    [Fact]
    public void Policy_DefaultsToNull()
    {
        var attribute = new AuthenticatedAttribute();
        attribute.Policy.ShouldBeNull();
    }

    [Fact]
    public void Policy_CanBeSet()
    {
        var attribute = new AuthenticatedAttribute { Policy = "RequireMfa" };
        attribute.Policy.ShouldBe("RequireMfa");
    }

    [Fact]
    public void Policy_CanBeSetAlongsideRoles()
    {
        var attribute = new AuthenticatedAttribute("Admin") { Policy = "RequireMfa" };
        attribute.Policy.ShouldBe("RequireMfa");
        attribute.AuthorizedRoles.ShouldNotBeNull();
        attribute.AuthorizedRoles![0].ShouldBe("Admin");
    }

    [Authenticated("Admin")]
    private class TestClassWithAttribute
    {
    }
}
