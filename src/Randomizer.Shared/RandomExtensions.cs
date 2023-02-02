using System;

namespace Randomizer.Shared;

public static class RandomExtensions
{
    /// <summary>
    /// .NET Random has an issue where the first few numbers can be subject to patterns
    /// This function will skip over the first 10 results to help ensure randomness
    /// </summary>
    /// <param name="rnd"></param>
    /// <returns></returns>
    public static Random Sanitize(this Random rnd)
    {
        for (var i = 0; i < 10; i++)
        {
            rnd.Next();
        }
        return rnd;
    }
}
