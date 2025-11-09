using Muninn.Kernel.Models;

namespace Muninn.Kernel.Extensions;

public static class EntryExtensions
{
    public static string DecodeValue(this Entry entry)
    {
        return entry.Encoding.GetString(entry.Value);
    }
}
