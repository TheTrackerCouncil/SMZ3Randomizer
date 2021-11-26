using System;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Provides positional arguments to string formatting functions.
    /// </summary>
    public static class Args
    {
        /// <summary>
        /// Combines a formatting object with another set of formatting objects.
        /// </summary>
        /// <param name="arg0">The first object to format.</param>
        /// <param name="additionalArgs">Additional objects to format.</param>
        /// <returns>
        /// A new object array with <paramref name="arg0"/> in the first
        /// position, followed by the elements in <paramref
        /// name="additionalArgs"/>.
        /// </returns>
        public static object?[] Combine(object? arg0, params object?[] additionalArgs)
        {
            var args = new object?[1 + additionalArgs.Length];
            args[0] = arg0;
            Array.Copy(additionalArgs, 0, args, 1, additionalArgs.Length);
            return args;
        }
    }
}
