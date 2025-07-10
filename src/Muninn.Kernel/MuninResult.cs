using Muninn.Kernel.Models;

namespace Muninn.Kernel;

public record MuninResult(bool IsSuccessful, Entry? Entry, string Message = "", Exception? Exception = null, bool IsCancelled = false);
