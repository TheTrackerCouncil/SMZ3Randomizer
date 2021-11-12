using System;

using NUnit.Framework;

using Randomizer.SMZ3.Generation;

namespace Randomizer.SMZ3.Tests
{
    [TestFixture]
    public class ParseTests
    {
        [TestCase("ABCDEF", 0x00ABCDEF)]
        [TestCase("7FFFFFFF", int.MaxValue)]
        [TestCase("FFFFFFFF", -1)]
        [TestCase("80000000", int.MinValue)]
        [TestCase("0x0", 0)]
        [TestCase("0x7FFFFFFF", int.MaxValue)]
        public void AsHexConvertsHexadecimalNumbers(string input, int expected)
        {
            var result = Parse.AsHex(input, out var value);

            Assert.IsTrue(result);
            Assert.AreEqual(expected, value);
        }

        [TestCase("ABCDEFG")]
        [TestCase("0x0x0x0")]
        public void AsHexReturnsFalseIfInputIsNotValid(string input)
        {
            var result = Parse.AsHex(input, out _);

            Assert.IsFalse(result);
        }

        [TestCase("0xFFFFFFFFFFFFFFFF")]
        public void AsHexReturnsFalseIfInputOverflows(string input)
        {
            var result = Parse.AsHex(input, out _);

            Assert.IsFalse(result);
        }

        [TestCase("0", 0)]
        [TestCase("2147483647", int.MaxValue)]
        [TestCase("-2147483648", int.MinValue)]
        public void AsIntegerConvertsPlainIntegers(string input, int expected)
        {
            var result = Parse.AsInteger(input, out var value);

            Assert.IsTrue(result);
            Assert.AreEqual(expected, value);
        }

        [TestCase("1.234.567,89")] // Thousands separators and decimal separators aren't allowed
        [TestCase("1,234,567.89")] // Thousands separators and decimal separators aren't allowed
        [TestCase("0xFF")] // Not a decimal integer
        public void AsIntegerReturnsFalseInvalidInts(string input)
        {
            var result = Parse.AsInteger(input, out _);

            Assert.IsFalse(result);
        }
    }
}
