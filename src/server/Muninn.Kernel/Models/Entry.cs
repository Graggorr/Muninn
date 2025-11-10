using System.Text;

namespace Muninn.Kernel.Models;

public sealed class Entry(string key, byte[] value, Encoding encoding)
{
    public int Hashcode { get; } = key.GetHashCode();

    public string Key { get; init; } = key;

    public byte[] Value { get; set; } = value;

    public TimeSpan LifeTime { get; set; } = TimeSpan.Zero;

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    public DateTime LastModificationTime { get; set; } = DateTime.UtcNow;

    public Encoding Encoding { get; set; } = encoding;

    public void Update(Entry entry)
    {
        Value = entry.Value;
        LifeTime = entry.LifeTime;
        LastModificationTime = entry.LastModificationTime;
        Encoding = entry.Encoding;
    }

    public Entry Clone() => new((string)Key.Clone(), (byte[])Value.Clone(), Encoding)
    {
        LifeTime = LifeTime,
        LastModificationTime = LastModificationTime,
        CreationTime = CreationTime,
    };
}
