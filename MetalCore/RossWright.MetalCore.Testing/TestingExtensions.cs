using RossWright;

/// <summary>
/// Extension methods for configuring Metal options builders in test scenarios.
/// </summary>
public static class TestingExtensions
{
    /// <summary>
    /// Directly sets the <see cref="AssemblyScanningOptionsBuilder.DiscoveredConcreteTypes"/> collection,
    /// bypassing real assembly scanning so tests control exactly which types are visible to the builder.
    /// </summary>
    /// <param name="assemblyScanningBuilder">The options builder to configure.</param>
    /// <param name="types">The concrete types to inject as discovered types.</param>
    public static void SetDiscoveredConcreteTypesForTesting(
        this AssemblyScanningOptionsBuilder assemblyScanningBuilder,
        params Type[] types)
    {
        assemblyScanningBuilder.DiscoveredConcreteTypes = types;
    }
}
