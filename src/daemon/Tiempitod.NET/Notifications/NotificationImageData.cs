namespace Tiempitod.NET.Notifications;

/// <summary>
/// D-Bus structured specified by freedesktop linux specification.
/// </summary>
public struct NotificationImageData
{
    /// <summary>
    /// Width of image in pixels
    /// </summary>
    public int Width { get; set; }
    
    /// <summary>
    /// Height of image in pixels
    /// </summary>
    public int Height { get; set; }
    
    /// <summary>
    /// Distance in bytes between row starts
    /// </summary>
    public int RowStride { get; set; }
    
    /// <summary>
    /// Whether the image has an alpha channel
    /// </summary>
    public bool HasAlpha { get; set; }
    
    /// <summary>
    /// Must always be 8.
    /// </summary>
    public int BitsPerSample { get; set; }
    
    /// <summary>
    /// If HasAlpha is TRUE, must be 4, otherwise 3
    /// </summary>
    public int Channels { get; set; }
    
    /// <summary>
    /// The image data, in RGB byte order.
    /// </summary>
    public byte[] Data { get; set; }
}
