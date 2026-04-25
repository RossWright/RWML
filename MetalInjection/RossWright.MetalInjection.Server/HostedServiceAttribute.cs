namespace RossWright.MetalInjection;

/// <summary>
/// Marker interface that registers the implementing class as a hosted background service
/// during MetalInjection assembly scanning. An alternative to decorating the class with
/// <see cref="HostedServiceAttribute"/>.
/// </summary>
/// <typeparam name="T">Unused type parameter; present for consistency with other MetalInjection marker interfaces such as <see cref="ISingleton{T}"/>.</typeparam>
public interface IHostedService<T> { }

/// <summary>
/// Marks a <see cref="Microsoft.Extensions.Hosting.BackgroundService"/> subclass for automatic
/// registration as a hosted service during MetalInjection assembly scanning via
/// <see cref="MetalInjectionServerExtensions.AddMetalInjection"/>.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
public class HostedServiceAttribute : Attribute
{
}