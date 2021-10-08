using System;

using NUnit.Framework;

namespace Randomizer.SMZ3.Tests
{
    [TestFixture]
    public class RandomizerTests
    {
        [TestCase("0", 0)]
        [TestCase("600233615", 600233615)]
        [TestCase("69217125", 69217125)]
        public void ParseSeedAcceptsRegularIntegers(string input, int expected)
        {
            var result = Generation.Randomizer.ParseSeed(ref input);

            Assert.AreEqual(expected, result);
        }

        [TestCase("23C6D68F", 600233615)]
        [TestCase("0x69217125", 1763799333)]
        public void ParseSeedAcceptsHexadecimalSeeds(string input, int expected)
        {
            var result = Generation.Randomizer.ParseSeed(ref input);

            Assert.AreEqual(expected, result);
        }

        [TestCase("Cas'", 1498539627)]
        public void ParseSeedAcceptsAnyString(string input, int expected)
        {
            var result = Generation.Randomizer.ParseSeed(ref input);

            Assert.AreEqual(expected, result);
        }

        [Test]
        public void ParseSeedReplacesEmptyInputWithRandomSeed()
        {
            var input = string.Empty;
            var result = Generation.Randomizer.ParseSeed(ref input);

            Assert.IsTrue(int.TryParse(input, out var seed));
            Assert.AreEqual(seed, result);
        }
    }
}
