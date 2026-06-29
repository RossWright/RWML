using System.ComponentModel.DataAnnotations;

namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Thrown when a customer with the given email address already exists.</summary>
public class DuplicateEmailException : ValidationException
{
    public string Email { get; }

    public DuplicateEmailException(string email)
        : base($"A customer with email '{email}' already exists.")
    {
        Email = email;
    }

    public DuplicateEmailException(string message, bool _) : base(message) { Email = string.Empty; }
}
