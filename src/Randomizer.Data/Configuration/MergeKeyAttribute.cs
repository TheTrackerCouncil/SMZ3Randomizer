using System;
using Randomizer.Shared;

namespace Randomizer.Data.Configuration
{
    /// <summary>
    /// Specifies the item categories for an item type.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property,
        Inherited = false, AllowMultiple = false)]
    public sealed class MergeKeyAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="MergeKeyAttribute"/> class with the specified categories.
        /// </summary>
        public MergeKeyAttribute() { }
    }
}
