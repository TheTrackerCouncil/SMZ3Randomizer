using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Randomizer.Shared
{
    /// <summary>
    /// Provides extension methods for <see cref="Type"/> objects.
    /// </summary>
    public static class TypeExtensions
    {
        /// <summary>
        /// Returns the values of all properties with the specified type.
        /// </summary>
        /// <typeparam name="TProperty">The type of property whose values to return.</typeparam>
        /// <param name="type">The type whose properties to find.</param>
        /// <param name="instance">The instance used to get the properties' values.</param>
        /// <param name="bindingFlags">The binding flags to use.</param>
        /// <returns>A collection of values of properties of the specified property type.</returns>
        public static IEnumerable<TProperty> GetPropertyValues<TProperty>(this Type type,
            object instance,
            BindingFlags bindingFlags = BindingFlags.Public | BindingFlags.Instance)
        {
            return type.GetProperties(bindingFlags)
                .Where(x => x.PropertyType.IsAssignableTo(typeof(TProperty)))
                .Select(x => (TProperty?)x.GetValue(instance))
                .NonNull();
        }
    }
}
