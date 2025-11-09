namespace Muninn
{
    /// <summary>
    /// Returns result of Muninn request
    /// </summary>
    /// <param name="isSuccessful">Marks if requests was successful or not</param>
    /// <param name="errorMessage">Message of the error in case of unsuccessful result</param>
    public record MuninnResult(bool isSuccessful, string errorMessage)
    {
        /// <summary>
        /// Marks if requests was successful or not
        /// </summary>
        public bool isSuccessful { get; } = isSuccessful;

        /// <summary>
        /// Message of the error in case of unsuccessful result
        /// </summary>
        public string errorMessage { get; } = errorMessage;
    }

    /// <summary>
    /// Returns result of Muninn request with <paramref name="Value"/>
    /// </summary>
    /// <typeparam name="T">Generic type of the value</typeparam>
    /// <param name="isSuccessful">Marks if requests was successful or not</param>
    /// <param name="errorMessage">Message of the error in case of unsuccessful result</param>
    public record MuninnResult<T>(bool isSuccessful, string errorMessage, T? Value) : MuninnResult(isSuccessful, errorMessage)
    {
        /// <summary>
        /// Actual value. Null in case of IsSuccessful is false
        /// </summary>
        public T? Value { get; } = Value;
    }
}
