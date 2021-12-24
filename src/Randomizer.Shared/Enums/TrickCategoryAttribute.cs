using System;
using Randomizer.Shared;

namespace Randomizer.Shared
{
    /// <summary>
    /// Specifies the trick categories for a trick
    /// </summary>
    [AttributeUsage(AttributeTargets.Field,
        Inherited = false, AllowMultiple = false)]
    public sealed class TrickCategoryAttribute : Attribute
    {
        /// <summary>
        /// Initializes a new instance of the <see
        /// cref="TrickCategoryAttribute"/> class with the specified categories.
        /// </summary>
        /// <param name="categories">
        /// The categories to assign to the trick.
        /// </param>
        public TrickCategoryAttribute(params TrickCategory[] categories)
        {
            Categories = categories;
        }

        /// <summary>
        /// Gets the categories for the type of item.
        /// </summary>
        public TrickCategory[] Categories { get; }
    }
}
