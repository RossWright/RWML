namespace RossWright.MetalNexus.Testbed.Shared;

/// <summary>Thrown when a requested customer ID does not exist.</summary>
public class CustomerNotFoundException : Exception
{
    public int CustomerId { get; }

    public CustomerNotFoundException(int customerId)
        : base($"Customer {customerId} was not found.")
    {
        CustomerId = customerId;
    }

    public CustomerNotFoundException(string message) : base(message) { }
}
