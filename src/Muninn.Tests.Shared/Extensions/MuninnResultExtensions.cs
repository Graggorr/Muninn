using Muninn.Kernel.Models;
using Shouldly;

namespace Muninn.Tests.Shared.Extensions;

public static class MuninnResultExtensions
{
    public static void ShouldBeSuccessful(this MuninnResult result, Entry entry)
    {
        result.IsSuccessful.ShouldBeTrue();
        result.Exception.ShouldBeNull();
        result.Entry.ShouldBe(entry);
    }

    public static void ShouldBeSuccessful(this MuninnResult result)
    {
        result.IsSuccessful.ShouldBeTrue();
        result.Exception.ShouldBeNull();
        result.Entry.ShouldBeNull();
    }
    
    public static void ShouldBeSuccessfulAndEquivalentTo(this MuninnResult result, Entry entry)
    {
        result.IsSuccessful.ShouldBeTrue();
        result.Exception.ShouldBeNull();
        result.Entry.ShouldBeEquivalentTo(entry);
    }

    public static void ShouldBeCancelled(this MuninnResult result)
    {
        result.IsSuccessful.ShouldBeFalse();
        result.IsCancelled.ShouldBeTrue();
        result.Entry.ShouldBeNull();
        result.Exception.ShouldBeAssignableTo<OperationCanceledException>();
    }
    
    public static void ShouldBeFailed(this MuninnResult result)
    {
        result.IsSuccessful.ShouldBeFalse();
        result.IsCancelled.ShouldBeFalse();
        result.Entry.ShouldBeNull();
        result.Exception.ShouldNotBeAssignableTo<OperationCanceledException>();
    }
}