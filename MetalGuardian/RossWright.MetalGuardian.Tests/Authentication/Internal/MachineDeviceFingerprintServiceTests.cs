using RossWright.MetalGuardian.Authentication;

namespace RossWright.MetalGuardian.Tests.Authentication.Internal;

public class MachineDeviceFingerprintServiceTests
{
    [Fact]
    public async Task GetFingerprint_WhenCalled_ReturnsNonEmptyString()
    {
        // Arrange
        var service = new MachineDeviceFingerprintService();

        // Act
        var fingerprint = await service.GetFingerprint();

        // Assert
        fingerprint.ShouldNotBeNullOrEmpty();
    }

    [Fact]
    public async Task GetFingerprint_WhenCalled_ReturnsLowercaseHexString()
    {
        // Arrange
        var service = new MachineDeviceFingerprintService();

        // Act
        var fingerprint = await service.GetFingerprint();

        // Assert
        fingerprint.Length.ShouldBe(64); // SHA-256 produces 64 hex characters
        fingerprint.ShouldMatch("^[0-9a-f]{64}$");
    }

    [Fact]
    public async Task GetFingerprint_WhenCalledMultipleTimes_ReturnsSameValue()
    {
        // Arrange
        var service = new MachineDeviceFingerprintService();

        // Act
        var fingerprint1 = await service.GetFingerprint();
        var fingerprint2 = await service.GetFingerprint();

        // Assert
        fingerprint1.ShouldBe(fingerprint2);
    }

    [Fact]
    public async Task GetFingerprint_WithDifferentInstances_ReturnsSameValue()
    {
        // Arrange
        var service1 = new MachineDeviceFingerprintService();
        var service2 = new MachineDeviceFingerprintService();

        // Act
        var fingerprint1 = await service1.GetFingerprint();
        var fingerprint2 = await service2.GetFingerprint();

        // Assert
        fingerprint1.ShouldBe(fingerprint2);
    }
}
