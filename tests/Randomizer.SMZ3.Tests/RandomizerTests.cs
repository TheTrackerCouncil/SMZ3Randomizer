using System;

using FluentAssertions;

using Xunit;

namespace Randomizer.SMZ3.Tests
{
    public class RandomizerTests
    {
        [Theory]
        [InlineData("0", 0)]
        [InlineData("600233615", 600233615)]
        [InlineData("69217125", 69217125)]
        public void ParseSeedAcceptsRegularIntegers(string? input, int expected)
        {
            var result = Generation.Smz3Randomizer.ParseSeed(ref input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("23C6D68F", 600233615)]
        [InlineData("0x69217125", 1763799333)]
        public void ParseSeedAcceptsHexadecimalSeeds(string? input, int expected)
        {
            var result = Generation.Smz3Randomizer.ParseSeed(ref input);

            Assert.Equal(expected, result);
        }

        [Theory]
        [InlineData("Cas'", 1498539627)]
        public void ParseSeedAcceptsAnyString(string? input, int expected)
        {
            var result = Generation.Smz3Randomizer.ParseSeed(ref input);

            Assert.Equal(expected, result);
        }

        [Fact]
        public void ParseSeedReplacesEmptyInputWithRandomSeed()
        {
            var input = string.Empty;
            var result = Generation.Smz3Randomizer.ParseSeed(ref input);

            Assert.True(int.TryParse(input, out var seed));
            Assert.Equal(seed, result);
        }
    }
}
