using Muninn.Kernel.Models;
using System.Text;

namespace Muninn.Kernel.Extensions;

public static class EntryExtensions
{
    public static string DecodeValue(this Entry entry)
    {
        var encoding = Encoding.GetEncoding(entry.EncodingName);

        return encoding.GetString(entry.Value);
    }
}
