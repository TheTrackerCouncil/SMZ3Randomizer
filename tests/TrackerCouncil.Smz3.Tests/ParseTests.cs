using TrackerCouncil.Smz3.SeedGenerator.Generation;
using Xunit;

namespace TrackerCouncil.Smz3.Tests
{
    public class ParseTests
    {
        [Theory]
        [InlineData("ABCDEF", 0x00ABCDEF)]
        [InlineData("7FFFFFFF", int.MaxValue)]
        [InlineData("FFFFFFFF", -1)]
        [InlineData("80000000", int.MinValue)]
        [InlineData("0x0", 0)]
        [InlineData("0x7FFFFFFF", int.MaxValue)]
        public void AsHexConvertsHexadecimalNumbers(string input, int expected)
        {
            var result = Parse.AsHex(input, out var value);

            Assert.True(result);
            Assert.Equal(expected, value);
        }

        [Theory]
        [InlineData("ABCDEFG")]
        [InlineData("0x0x0x0")]
        public void AsHexReturnsFalseIfInputIsNotValid(string input)
        {
            var result = Parse.AsHex(input, out _);

            Assert.False(result);
        }

        [Theory]
        [InlineData("0xFFFFFFFFFFFFFFFF")]
        public void AsHexReturnsFalseIfInputOverflows(string input)
        {
            var result = Parse.AsHex(input, out _);

            Assert.False(result);
        }

        [Theory]
        [InlineData("0", 0)]
        [InlineData("2147483647", int.MaxValue)]
        [InlineData("-2147483648", int.MinValue)]
        public void AsIntegerConvertsPlainIntegers(string input, int expected)
        {
            var result = Parse.AsInteger(input, out var value);

            Assert.True(result);
            Assert.Equal(expected, value);
        }

        [Theory]
        [InlineData("1.234.567,89")] // Thousands separators and decimal separators aren't allowed
        [InlineData("1,234,567.89")] // Thousands separators and decimal separators aren't allowed
        [InlineData("0xFF")] // Not a decimal integer
        public void AsIntegerReturnsFalseInvalidInts(string input)
        {
            var result = Parse.AsInteger(input, out _);

            Assert.False(result);
        }
    }
}
