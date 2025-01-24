using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace Tiempitod.NET.Notifications.Linux;

/// <summary>
/// Represents the icon loader for linux operating systems.
/// </summary>
public class LinuxSystemIconLoader : ISystemAsyncIconLoader
{
    public async Task<NotificationImageData> LoadAsync(string iconPath)
    {
        using Image<Rgba32> image = await Image.LoadAsync<Rgba32>(iconPath);
        
        bool hasAlpha = image.PixelType.AlphaRepresentation is null or PixelAlphaRepresentation.None;
        int channels = hasAlpha ? 4 : 3;
        int rowStride = channels * image.Width;

        byte[] data = GetDataArray(image, rowStride, hasAlpha, channels);
        
        return new NotificationImageData
        {
            Width = image.Width,
            Height = image.Height,
            RowStride = rowStride,
            BitsPerSample = 8,
            Channels = hasAlpha ? 4 : 3,
            HasAlpha = hasAlpha,
            Data = data
        };
    }

    /// <summary>
    /// Gets an array of bytes representing the data of the image in RGB order.
    /// </summary>
    /// <param name="img">A <see cref="Image"/> to read the data from.</param>
    /// <param name="rowStride">Length of byte rows.</param>
    /// <param name="hasAlpha">If the image has alpha channel.</param>
    /// <param name="channels">4 channels if it has alpha, 3 otherwise.</param>
    /// <returns>An array of bytes with the data.</returns>
    private static byte[] GetDataArray(Image<Rgba32> img, int rowStride, bool hasAlpha, int channels)
    {
        var data = new byte[rowStride * img.Height];
        
        img.ProcessPixelRows(accessor =>
        {
            for (var y = 0; y < accessor.Height; y++)
            {
                Span<Rgba32> row = accessor.GetRowSpan(y);
                for (var x = 0; x < row.Length; x++)
                {
                    int offset = y * rowStride + x * channels;
                    
                    data[offset] = row[x].R;
                    data[offset + 1] = row[x].G;
                    data[offset + 2] = row[x].B;

                    if (hasAlpha)
                        data[offset + 3] = row[x].A;
                }
            }
        });
        
        return data;
    }
}