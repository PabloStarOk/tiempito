using System.IO.Pipes;
using System.Text;

namespace Tiempitod.NET.Configuration;

public class PipeConfig
{
    public const string Pipe = "Pipe";

    public string PipeName { get; set; } = "tiempito-pipe";
    public PipeDirection PipeDirection { get; set; } = PipeDirection.InOut;
    public int PipeMaxInstances { get; set; } = 1;
    public string PipeEncoding { get; set; } = "utf8";
    public int MaxRestartAttempts { get; set; } = 3;
    public bool DisplayImpersonationUser { get; set; } = true;

    public Type GetEncodingType()
    {
        string formattedEncoding = PipeEncoding.ToLower().Replace("-", "");

        return formattedEncoding switch
        {
            "utf8" => typeof(UTF8Encoding),
            "utf7" => typeof(UTF7Encoding),
            "ascii" => typeof(ASCIIEncoding),
            "utf32" => typeof(UTF32Encoding),
            "unicode" => typeof(UnicodeEncoding),
            _ => typeof(UTF8Encoding)
        };
    }
}
