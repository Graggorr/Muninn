namespace Muninn
{
    /// <summary>
    /// Returns result of Muninn request
    /// </summary>
    /// <param name="IsSuccessful">Marks if requests was successful or not</param>
    /// <param name="ErrorMessage">Message of the error in case of unsuccessful result</param>
    public record MuninnResult(bool IsSuccessful, string ErrorMessage)
    {
        /// <summary>
        /// Marks if requests was successful or not
        /// </summary>
        public bool IsSuccessful { get; } = IsSuccessful;

        /// <summary>
        /// Message of the error in case of unsuccessful result
        /// </summary>
        public string ErrorMessage { get; } = ErrorMessage;
    }

    /// <summary>
    /// Returns result of Muninn request with <paramref name="Value"/>
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="IsSuccessful">Marks if requests was successful or not</param>
    /// <param name="ErrorMessage">Message of the error in case of unsuccessful result</param>
    public record MuninnResult<T>(bool IsSuccessful, string ErrorMessage, T? Value) : MuninnResult(IsSuccessful, ErrorMessage)
    {
        /// <summary>
        /// Actual value. Null in case of IsSuccessful is false
        /// </summary>
        public T? Value { get; } = Value;
    }
}
