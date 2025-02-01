using Tmds.DBus.Protocol;

namespace Tiempitod.NET.Notifications.Objects;

/// <summary>
/// D-Bus structured specified by freedesktop linux specification.
/// </summary>
public readonly struct NotificationImageData
{
    /// <summary>
    /// Width of image in pixels
    /// </summary>
    public int Width { get; init; }
    
    /// <summary>
    /// Height of image in pixels
    /// </summary>
    public int Height { get; init; }
    
    /// <summary>
    /// Distance in bytes between row starts
    /// </summary>
    public int RowStride { get; init; }
    
    /// <summary>
    /// Whether the image has an alpha channel
    /// </summary>
    public bool HasAlpha { get; init; }
    
    /// <summary>
    /// Must always be 8.
    /// </summary>
    public int BitsPerSample { get; init; }
    
    /// <summary>
    /// If HasAlpha is TRUE, must be 4, otherwise 3
    /// </summary>
    public int Channels { get; init; }
    
    /// <summary>
    /// The image data, in RGB byte order.
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
