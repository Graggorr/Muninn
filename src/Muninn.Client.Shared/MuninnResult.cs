namespace Muninn;

/// <summary>
/// Returns result of Muninn request
/// </summary>
/// <param name="IsSuccessful">Marks if requests was successful or not</param>
public record MuninnResult(bool IsSuccessful);

/// <summary>
/// Returns result of Muninn request
/// </summary>
/// <typeparam name="T">Generic type of the value</typeparam>
/// <param name="IsSuccessful">Marks if requests was successful or not</param>
/// <param name="Value">Actual value. Null in case of IsSuccessful is false</param>
public record MuninnResult<T>(bool IsSuccessful, T? Value) : MuninnResult(IsSuccessful);
