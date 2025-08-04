using System.Text;

namespace Muninn;

public class MuninnConfiguration
{
    public string EncodingName { get; set; } = Encoding.ASCII.EncodingName;

    public string HostName { get; set; } = string.Empty;

    public string ApiKey { get; set; } = string.Empty;
}
