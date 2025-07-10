namespace Muninn.Kernel.Models;

public enum Condition : short
{
    And = 0,
    Or = 1,
    Not = 2,
}

public readonly ref struct KeyFilter(string value, Condition condition)
{
    public string Value { get; init; } = value;

    public Condition Condition { get; init; } = condition;
}
