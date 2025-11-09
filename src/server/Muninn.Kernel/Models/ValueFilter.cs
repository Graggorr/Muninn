namespace Muninn.Kernel.Models;

public readonly struct ValueFilter(string value, Condition condition)
{
    public string Value { get; init; } = value;

    public Condition Condition { get; init; } = condition;
}
