namespace Muninn.Kernel.Persistent;

public class PersistentConfiguration
{
    public string DirectoryPath { get; set; } = Directory.GetCurrentDirectory();

    public int DefaultBufferSize { get; set; } = 4096;
}
