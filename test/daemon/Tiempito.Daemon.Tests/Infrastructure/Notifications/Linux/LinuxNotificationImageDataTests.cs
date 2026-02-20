using Tiempito.Daemon.Infrastructure.Notifications.Linux;

using Tmds.DBus.Protocol;

namespace Tiempito.Daemon.Tests.Infrastructure.Notifications.Linux;

/// <summary>
/// Unit tests for the <see cref="LinuxNotificationImageData"/> struct.
/// </summary>
[Trait("Category", "Unit")]
[Trait("Feature", "Notifications")]
public sealed class LinuxNotificationImageDataTests
{
    /// <summary>
    /// Tests <see cref="LinuxNotificationImageData.GetVariantValue"/> returns a
    /// <see cref="Tmds.DBus.Protocol.VariantValue"/> struct that contains the
    /// image fields (width, height, row stride, alpha flag, bits per sample,
    /// channels and raw data) in the expected order and values.
    /// </summary>
    [Fact]
    public void GetVariantValue_should_ReturnExpectedVariantValue()
    {
        // Arrange
        var imgData = new LinuxNotificationImageData
        {
            Width = 300,
            Height = 300,
            Channels = 4,
            BitsPerSample = 8,
            Data = "Some fake RGB data"u8.ToArray(),
            HasAlpha = true,
            RowStride = 8,
        };
        VariantValue width = VariantValue.Int32(imgData.Width);
        VariantValue height = VariantValue.Int32(imgData.Height);
        VariantValue rowStride = VariantValue.Int32(imgData.RowStride);
        VariantValue hasAlpha = VariantValue.Bool(imgData.HasAlpha);
        VariantValue bitsPerSample = VariantValue.Int32(imgData.BitsPerSample);
        VariantValue channels = VariantValue.Int32(imgData.Channels);
        VariantValue data = VariantValue.Array(imgData.Data);
        var expected = VariantValue.Struct(width, height, rowStride, hasAlpha, bitsPerSample, channels, data);

        // Act
        var actual = imgData.GetVariantValue();

        // Assert
        Assert.Equal(expected.ToString(), actual.ToString());
    }
}