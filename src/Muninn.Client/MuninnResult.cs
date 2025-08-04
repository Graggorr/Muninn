namespace Muninn;

public record MuninnResult<T>(bool IsSuccessful, T? Value);
