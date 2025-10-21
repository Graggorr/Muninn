namespace Muninn.Kernel.Models;

public record MuninnResult(bool IsSuccessful, Entry? Entry, string Message = "", Exception? Exception = null, bool IsCancelled = false);

