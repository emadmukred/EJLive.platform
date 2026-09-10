using EJLive.Core.Engine;
using EJLive.Core.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace EJLive.Tests.Track14;

[TestClass]
public class ScreenshotTests
{
    private static ScreenshotCaptureService CreateService(
        IScreenCaptureProvider? provider = null,
        IImageEncoder? encoder = null,
        IScreenshotDeliveryClient? delivery = null,
        IScreenshotRetentionPolicy? retention = null)
    {
        return new ScreenshotCaptureService(
            provider ?? new TestCaptureProvider(),
            encoder ?? new TestImageEncoder(),
            delivery ?? new TestDeliveryClient(),
            retention ?? new TestRetentionPolicy());
    }

    [TestMethod]
    public async Task CaptureAndDeliverAsync_NoDesktopSession0_ReturnsFailure()
    {
        // Arrange
        var provider = new TestCaptureProvider(available: false);
        var service = CreateService(provider);

        // Act
        var result = await service.CaptureAndDeliverAsync("ATM001");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
        StringAssert.Contains(result.ErrorMessage, "No desktop");
    }

    [TestMethod]
    public async Task CaptureAndDeliverAsync_LargeScreen_CompressesWithinCap()
    {
        // Arrange
        var provider = new TestCaptureProvider(
            width: 3840,
            height: 2160,
            dataSize: 2_000_000);
        var delivery = new TestDeliveryClient();
        var service = CreateService(provider, delivery: delivery);

        // Act
        var result = await service.CaptureAndDeliverAsync("ATM001", new CaptureOptions { MaxSizeBytes = 500_000 });

        // Assert
        Assert.IsTrue(result.Success);
        Assert.IsNotNull(result.Metadata);
        Assert.IsTrue(result.Metadata.SizeBytes <= 500_000, "Compressed size should not exceed cap.");
    }

    [TestMethod]
    public async Task CaptureAndDeliverAsync_CaptureFailure_ReturnsFailure()
    {
        // Arrange
        var provider = new FailingCaptureProvider();
        var service = CreateService(provider);

        // Act
        var result = await service.CaptureAndDeliverAsync("ATM001");

        // Assert
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.ErrorMessage);
        StringAssert.Contains(result.ErrorMessage, "Capture failed");
    }

    [TestMethod]
    public async Task CaptureAndDeliverAsync_RetentionCleanup_IsInvoked()
    {
        // Arrange
        var retention = new TestRetentionPolicy();
        var service = CreateService(retention: retention);

        // Act
        await service.CaptureAndDeliverAsync("ATM001");

        // Assert
        Assert.IsTrue(retention.CleanupInvoked);
        Assert.AreEqual("ATM001", retention.LastAtmId);
    }

    // Test helpers

    private class TestCaptureProvider : IScreenCaptureProvider
    {
        private readonly bool _available;
        private readonly int _width;
        private readonly int _height;
        private readonly int _dataSize;

        public TestCaptureProvider(bool available = true, int width = 1920, int height = 1080, int dataSize = 100_000)
        {
            _available = available;
            _width = width;
            _height = height;
            _dataSize = dataSize;
        }

        public bool IsDesktopAvailable() => _available;

        public Task<RawImageData> CaptureAsync(CancellationToken cancellationToken = default)
        {
            return Task.FromResult(new RawImageData
            {
                Data = new byte[_dataSize],
                Width = _width,
                Height = _height
            });
        }
    }

    private class FailingCaptureProvider : IScreenCaptureProvider
    {
        public bool IsDesktopAvailable() => true;

        public Task<RawImageData> CaptureAsync(CancellationToken cancellationToken = default)
        {
            throw new InvalidOperationException("Simulated capture device failure.");
        }
    }

    private class TestImageEncoder : IImageEncoder
    {
        public byte[] Encode(byte[] rawData, int width, int height, int quality)
        {
            // Simulate compression: smaller quality -> smaller output
            int targetSize = Math.Max(1, rawData.Length * quality / 100);
            return new byte[targetSize];
        }
    }

    private class TestDeliveryClient : IScreenshotDeliveryClient
    {
        public List<(ScreenshotMetadata Metadata, byte[] Payload)> Deliveries { get; } = new();

        public Task SendAsync(ScreenshotMetadata metadata, byte[] payload, CancellationToken cancellationToken = default)
        {
            Deliveries.Add((metadata, payload));
            return Task.CompletedTask;
        }
    }

    private class TestRetentionPolicy : IScreenshotRetentionPolicy
    {
        public bool CleanupInvoked { get; private set; }
        public string? LastAtmId { get; private set; }

        public Task CleanupAsync(string atmId, CancellationToken cancellationToken = default)
        {
            CleanupInvoked = true;
            LastAtmId = atmId;
            return Task.CompletedTask;
        }
    }
}
