using Muninn.Kernel.Models;

namespace Muninn.Server.Tests.Extensions.cs;

public static class EntryExtensions
{
    public static Entry Clone(this Entry entry, byte[]? value = null) => new(entry.Key, value ?? entry.Value, entry.Encoding, entry.LifeTime);
}
