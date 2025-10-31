using Tmds.DBus.Protocol;

namespace Tiempito.Daemon.Application.Notifications;

/// <summary>
/// D-Bus structured specified by freedesktop linux specification.
/// </summary>
public readonly struct NotificationImageData
{
    /// <summary>
    /// Gets width of image in pixels.
    /// </summary>
    public int Width { get; init; }

    /// <summary>
    /// Gets height of image in pixels.
    /// </summary>
    public int Height { get; init; }

    /// <summary>
    /// Gets distance in bytes between row starts.
    /// </summary>
    public int RowStride { get; init; }

    /// <summary>
    /// Gets a value indicating whether the image has an alpha channel.
    /// </summary>
    public bool HasAlpha { get; init; }

    /// <summary>
    /// Gets must always be 8.
    /// </summary>
    public int BitsPerSample { get; init; }

    /// <summary>
    /// Gets if HasAlpha is TRUE, must be 4, otherwise 3.
    /// </summary>
    public int Channels { get; init; }

    /// <summary>
    /// Gets the image data, in RGB byte order.
    /// </summary>
    public byte[] Data { get; init; }

    /// <summary>
    /// Gets the <see cref="VariantValue"/> structure required for the notifications dbus.
    /// </summary>
    /// <returns>A <see cref="VariantValue"/> DBus structure.</returns>
    public VariantValue GetVariantValue()
    {
        VariantValue width = VariantValue.Int32(Width);
        VariantValue height = VariantValue.Int32(Height);
        VariantValue rowStride = VariantValue.Int32(RowStride);
        VariantValue hasAlpha = VariantValue.Bool(HasAlpha);
        VariantValue bitsPerSample = VariantValue.Int32(BitsPerSample);
        VariantValue channels = VariantValue.Int32(Channels);
        VariantValue data = VariantValue.Array(Data);

        return VariantValue.Struct(width, height, rowStride, hasAlpha, bitsPerSample, channels, data);
    }
}
