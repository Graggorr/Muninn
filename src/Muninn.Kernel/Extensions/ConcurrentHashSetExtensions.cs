using Muninn.Kernel.Shared;

namespace Muninn.Kernel.Extensions;

public static class ConcurrentHashSetExtensions
{
    public static ConcurrentHashSet<T> ToConcurrentHashSet<T>(this IEnumerable<T> enumerable) => [..enumerable];
}
