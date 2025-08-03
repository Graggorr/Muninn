namespace Muninn.Kernel.Models;

internal class CleanupEntry(string key, DateTime expirationTime)
{
    public string Key { get; init; } = key;

    public DateTime ExpirationTime { get; set; } = expirationTime;
}
