using System;

namespace Randomizer.Shared;

[AttributeUsage(AttributeTargets.Class,
    Inherited = false, AllowMultiple = false)]
public class OrderAttribute : Attribute
{
    /// <summary>
    /// Initializes a new instance of the <see
    /// cref="OrderAttribute"/> class with the specified categories.
    /// </summary>
    /// <param name="order">
    /// The order in which the object is to be applied
    /// </param>
    public OrderAttribute(int order)
    {
        Order = order;
    }

    /// <summary>
    /// The order in which the object is to be applied
    /// </summary>
    public int Order { get; }
}
