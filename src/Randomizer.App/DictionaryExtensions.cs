using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Randomizer.App
{
    public static class DictionaryExtensions
    {
        public static void Increment<TKey>(this Dictionary<TKey, int> dictionary, TKey key) where TKey : notnull
        {
            if (!dictionary.ContainsKey(key))
            {
                dictionary.Add(key, 1);
            }
            else
            {
                dictionary[key]++;
            }
        }

        public static void Increment<TKey>(this ConcurrentDictionary<TKey, int> dictionary, TKey key) where TKey : notnull
        {
            dictionary.AddOrUpdate(key, 1, (_, current) => current + 1);
        }
    }
}
