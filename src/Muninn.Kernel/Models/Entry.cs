namespace Muninn.Kernel.Models;

public class Entry(string key, byte[] value) : IComparable<Entry>
{
    public int Hashcode { get; } = key.GetHashCode();

    public string Key { get; init; } = key;

    public byte[] Value { get; set; } = value;

    public TimeSpan LifeTime { get; set; } = TimeSpan.FromDays(1);

    public DateTime CreationTime { get; init; } = DateTime.UtcNow;

    public DateTime LastModificationTime { get; set; }

    public string EncodingName { get; set; } = "ASCII";

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
