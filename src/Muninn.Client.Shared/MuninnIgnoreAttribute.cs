namespace Muninn
{
    /// <summary>
    /// Marks the property or the field to be ignored in source generated binary serialization
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, Inherited = true, AllowMultiple = false)]
    public class MuninnIgnoreAttribute : Attribute;
}
