using System;

namespace TrackerCouncil.Smz3.Shared;

public static class RandomExtensions
{
    /// <param name="rnd"></param>
    extension(Random rnd)
    {
        /// <summary>
        /// .NET Random has an issue where the first few numbers can be subject to patterns
        /// This function will skip over the first 10 results to help ensure randomness
        /// </summary>
        /// <returns></returns>
        public Random Sanitize()
        {
            for (var i = 0; i < 10; i++)
            {
                rnd.Next();
            }
            return rnd;
        }

        /// <summary>
        /// Returns a random value from the provided enum
        /// </summary>
        /// <typeparam name="T">Enum type to return a random value from</typeparam>
        /// <returns>A random value from the provided enum</returns>
        public T Next<T>() where T : struct, Enum
        {
            var values = Enum.GetValues<T>();
            return values[rnd.Next(values.Length)];
        }

        /// <summary>
        /// Returns a random value from the provided enum excluding the last record in the enumd
        /// </summary>
        /// <typeparam name="T">Enum type to return a random value from</typeparam>
        /// <returns>A random value from the provided enum</returns>
        public T NextExcludingLast<T>() where T : struct, Enum
        {
            var values = Enum.GetValues<T>();
            return values.Length <= 1 ? values[0] : values[rnd.Next(values.Length - 1)];
        }
    }
}
