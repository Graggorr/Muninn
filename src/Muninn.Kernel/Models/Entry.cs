namespace Muninn.Kernel.Models;

public class Entry(string key, byte[] value)
{
    public int Hashcode { get; } = key.GetHashCode();

    public string Key { get; init; } = key;

    public byte[] Value { get; set; } = value;

    public TimeSpan LifeTime { get; set; } = TimeSpan.FromDays(1);

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    public DateTime LastModificationTime { get; set; }
}
