using System.Text;

namespace Muninn.Kernel.Models;

public class Entry(string key, byte[] value, Encoding encoding, TimeSpan lifeTime) : IComparable<Entry>
{
    public int Hashcode { get; } = key.GetHashCode();

    public string Key { get; init; } = key;

    public byte[] Value { get; set; } = value;

    public TimeSpan LifeTime { get; set; } = lifeTime;

    public DateTime CreationTime { get; set; } = DateTime.UtcNow;

    public DateTime LastModificationTime { get; set; } = DateTime.UtcNow;

    public Encoding Encoding { get; set; } = encoding;

    public static Entry Empty { get; } = new(string.Empty, [], Encoding.Default, TimeSpan.Zero);
    
    public static Entry CreateFilterEntry(string key) => new(key, [], Encoding.ASCII, default);

    public bool IsEmpty => CompareTo(Empty) is 0;
    
    public int CompareTo(Entry? other)
    {
        if (ReferenceEquals(this, other))
        {
            return 0;
        }

        if (other is null)
        {
            return 1;
        }

        return Hashcode.CompareTo(other.Hashcode);
    }
}
