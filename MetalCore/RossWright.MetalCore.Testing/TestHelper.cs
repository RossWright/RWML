using NSubstitute;
using System.Reflection;

/// <summary>
/// Provides helper utilities for unit tests that exercise assembly-scanning logic.
/// </summary>
public class TestHelper
{
    /// <summary>
    /// Creates a mock <see cref="System.Reflection.Assembly"/> whose <see cref="System.Reflection.Assembly.GetTypes"/> method
    /// returns exactly the supplied <paramref name="types"/>.
    /// Use this to isolate assembly-scanning tests from the real loaded assemblies.
    /// </summary>
    /// <param name="types">The types the mock assembly should expose.</param>
    /// <returns>A substitute <see cref="System.Reflection.Assembly"/> configured to return <paramref name="types"/>.</returns>
    public static Assembly SetupAssemblyWithTypes(params Type[] types)
    {
        Assembly mockAssembly = Substitute.For<Assembly>();
        mockAssembly.GetTypes().Returns(types);
        return mockAssembly;
    }
}