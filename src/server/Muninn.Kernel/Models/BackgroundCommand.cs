namespace Muninn.Kernel.Models;

internal class BackgroundCommand(string key, Entry? entry, BackgroundAction action)
{
    public string Key { get; set; } = key;
    
    public Entry? Entry { get; set; } = entry;
    public BackgroundAction Action { get; set; } = action;
}

internal enum BackgroundAction
{
    Add,
    Update,
    Remove,
    Skip,
}
