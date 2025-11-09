using System.Text;

namespace Muninn.Kernel.Models;

public class Entry(string key, byte[] value, Encoding encoding, TimeSpan lifeTime)
{
    public int Hashcode { get; } = key.GetHashCode();

    public string Key { get; init; } = key;

    public byte[] Value { get; set; } = value;

    public TimeSpan LifeTime { get; set; } = lifeTime;

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public DateTime LastModificationTime { get; set; } = DateTime.UtcNow;

    public Encoding Encoding { get; set; } = encoding;
}
