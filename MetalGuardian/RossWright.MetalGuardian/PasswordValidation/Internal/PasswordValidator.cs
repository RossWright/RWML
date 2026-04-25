namespace RossWright.MetalGuardian;

internal class PasswordValidator : IPasswordValidator
{
    public PasswordValidator(PasswordRequirements passwordRequirements) =>
        _passwordRequirements = passwordRequirements;
    private readonly PasswordRequirements _passwordRequirements;

    public string GetPasswordRequirementsMessage(params string?[] forbiddenFragments)
    {
        string? lengthReqs = null;
        if (_passwordRequirements.MinimumLength > 0)
        {
            if (_passwordRequirements.MaximumLength < int.MaxValue)
            {
                lengthReqs = $"Password must be between {_passwordRequirements.MinimumLength} and {_passwordRequirements.MaximumLength} characters";
            }
            else
            {
                lengthReqs = $"Password must be at least {_passwordRequirements.MinimumLength} characters";
            }
        }
        else if (_passwordRequirements.MaximumLength < int.MaxValue)
        {
            lengthReqs = $"Password must be less than {_passwordRequirements.MaximumLength} characters";
        }
                
        var charReqs = new string[]
        {
            _passwordRequirements.RequireUpperCase ? "one uppercase letter" : null!,
            _passwordRequirements.RequireLowerCase ? "one lowercase letter" : null!,
            _passwordRequirements.RequireDigit ? "one digit" : null!,
            _passwordRequirements.RequireSymbol ? "one symbol" : null!,
        }.Where(_ => _ != null)
        .ToArray();

        var forbidReqs = forbiddenFragments.CommaListJoin("or");
        if (forbidReqs != null) 
            forbidReqs = $"may not contain of the following: {forbidReqs}";

        if (!charReqs.Any())
        {
            if (forbidReqs == null)
            {
                return lengthReqs ?? "Password has no requirments";
            }
            else if (lengthReqs != null)
            {
                return $"{lengthReqs}, and {forbidReqs}";
            }
            else
            {
                return $"Password {forbidReqs}";
            }
        }
        else if (lengthReqs == null)
        {
            if (forbidReqs == null)
            {
                return $"Password must contain at least {charReqs.CommaListJoin()}";
            }
            else
            {
                return $"Password must contain at least {charReqs.CommaListJoin()}, and {forbidReqs}";
            }
        }
        else if(forbidReqs == null)
        {
            return $"{lengthReqs} and must contain at least {charReqs.CommaListJoin()}";
        }
        else
        {
            return $"{lengthReqs} and must contain at least {charReqs.CommaListJoin()}, and {forbidReqs}";
        }
    }

    public bool ValidatePassword(string? password, params string?[] forbiddenFragments)
    {
        if (password == null) return _passwordRequirements.MinimumLength == 0;
        if (password.Length < _passwordRequirements.MinimumLength) return false;
        if (password.Length > _passwordRequirements.MaximumLength) return false;
        if (_passwordRequirements.RequireUpperCase && !password.Any(char.IsUpper)) return false;
        if (_passwordRequirements.RequireLowerCase && !password.Any(char.IsLower)) return false;
        if (_passwordRequirements.RequireDigit && !password.Any(char.IsDigit)) return false;
        if (_passwordRequirements.RequireSymbol && !password.Any(_ => _passwordRequirements.AllowedSymbols.Contains(_))) return false;
        if (forbiddenFragments.Where(_ => !string.IsNullOrWhiteSpace(_))
            .Any(_ => password.Contains(_!, StringComparison.InvariantCultureIgnoreCase)))
        {
            return false;
        }
        return true;
    }
}
