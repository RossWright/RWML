using Shouldly;

namespace RossWright.MetalGuardian.Tests.PasswordValidation;

public class PasswordValidatorTests
{
    private static IPasswordValidator BuildValidator(Action<PasswordRequirements>? configure = null)
    {
        var req = new PasswordRequirements();
        configure?.Invoke(req);
        return new PasswordValidator(req);
    }

    [Fact]
    public void ValidPassword_ReturnsTrue()
    {
        var validator = BuildValidator();
        validator.ValidatePassword("Abcdef1!").ShouldBeTrue();
    }

    [Fact]
    public void BelowMinimumLength_ReturnsFalse()
    {
        var validator = BuildValidator(r => r.MinimumLength = 10);
        validator.ValidatePassword("Abcde1!").ShouldBeFalse();
    }

    [Fact]
    public void AboveMaximumLength_ReturnsFalse()
    {
        var validator = BuildValidator(r => r.MaximumLength = 8);
        validator.ValidatePassword("Abcdefg1!").ShouldBeFalse();
    }

    [Fact]
    public void MissingUppercase_ReturnsFalse()
    {
        var validator = BuildValidator(r => r.RequireUpperCase = true);
        validator.ValidatePassword("abcdef1!").ShouldBeFalse();
    }

    [Fact]
    public void MissingLowercase_ReturnsFalse()
    {
        var validator = BuildValidator(r => r.RequireLowerCase = true);
        validator.ValidatePassword("ABCDEF1!").ShouldBeFalse();
    }

    [Fact]
    public void MissingDigit_ReturnsFalse()
    {
        var validator = BuildValidator(r => r.RequireDigit = true);
        validator.ValidatePassword("Abcdefg!").ShouldBeFalse();
    }

    [Fact]
    public void MissingSymbol_ReturnsFalse()
    {
        var validator = BuildValidator(r => r.RequireSymbol = true);
        validator.ValidatePassword("Abcdef12").ShouldBeFalse();
    }

    [Fact]
    public void SymbolNotInAllowedList_ReturnsFalse()
    {
        var validator = BuildValidator(r =>
        {
            r.RequireSymbol = true;
            r.AllowedSymbols = ['!'];
        });
        // '#' is not in the restricted allowed list
        validator.ValidatePassword("Abcdef1#").ShouldBeFalse();
    }

    [Fact]
    public void NullPassword_MinLengthZero_ReturnsTrue()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 0;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        validator.ValidatePassword(null).ShouldBeTrue();
    }

    [Fact]
    public void NullPassword_MinLengthPositive_ReturnsFalse()
    {
        var validator = BuildValidator(r => r.MinimumLength = 1);
        validator.ValidatePassword(null).ShouldBeFalse();
    }

    [Fact]
    public void ForbiddenFragment_ReturnsFalse()
    {
        var validator = BuildValidator(r =>
        {
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
            r.MinimumLength = 0;
        });
        validator.ValidatePassword("password123", "PASSWORD").ShouldBeFalse();
    }

    [Fact]
    public void AllRequirementsDisabled_ReturnsTrue()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 0;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        validator.ValidatePassword("anything").ShouldBeTrue();
    }

    [Fact]
    public void GetRequirementsMessage_MinOnly()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 6;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage();
        msg.ShouldContain("6");
        msg.ShouldContain("least");
    }

    [Fact]
    public void GetRequirementsMessage_AllRequirements()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 8;
            r.RequireUpperCase = true;
            r.RequireLowerCase = true;
            r.RequireDigit = true;
            r.RequireSymbol = true;
        });
        var msg = validator.GetPasswordRequirementsMessage();
        msg.ShouldContain("8");
        msg.ShouldContain("uppercase");
        msg.ShouldContain("lowercase");
        msg.ShouldContain("digit");
        msg.ShouldContain("symbol");
    }

    [Fact]
    public void GetRequirementsMessage_WithForbiddenFragments()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 0;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage("admin", "password");
        msg.ShouldContain("admin");
        msg.ShouldContain("password");
    }

    [Fact]
    public void GetRequirementsMessage_NoRequirements()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 0;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage();
        msg.ShouldNotBeNullOrWhiteSpace();
    }

    [Fact]
    public void GetRequirementsMessage_MinAndMax_CoversLine16()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 6;
            r.MaximumLength = 20;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage();
        msg.ShouldBe("Password must be between 6 and 20 characters");
    }

    [Fact]
    public void GetRequirementsMessage_MaxOnly_CoversLine25()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 0;
            r.MaximumLength = 10;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage();
        msg.ShouldBe("Password must be less than 10 characters");
    }

    [Fact]
    public void GetRequirementsMessage_LengthAndForbidden_CoversLine49()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 8;
            r.RequireUpperCase = false;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage("admin", "password");
        msg.ShouldBe("Password must be at least 8 characters, and may not contain of the following: admin or password");
    }

    [Fact]
    public void GetRequirementsMessage_CharReqsOnly_CoversLine60()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 0;
            r.RequireUpperCase = true;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage();
        msg.ShouldBe("Password must contain at least one uppercase letter");
    }

    [Fact]
    public void GetRequirementsMessage_CharReqsAndForbidden_CoversLine64()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 0;
            r.RequireUpperCase = true;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage("test");
        msg.ShouldBe("Password must contain at least one uppercase letter, and may not contain of the following: test");
    }

    [Fact]
    public void GetRequirementsMessage_AllThree_CoversLine73()
    {
        var validator = BuildValidator(r =>
        {
            r.MinimumLength = 8;
            r.RequireUpperCase = true;
            r.RequireLowerCase = false;
            r.RequireDigit = false;
            r.RequireSymbol = false;
        });
        var msg = validator.GetPasswordRequirementsMessage("admin");
        msg.ShouldBe("Password must be at least 8 characters and must contain at least one uppercase letter, and may not contain of the following: admin");
    }
}
