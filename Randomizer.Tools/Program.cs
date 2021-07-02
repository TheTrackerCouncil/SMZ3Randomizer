using System;
using System.Collections.Generic;
using System.Linq;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Text;
using NHyphenator;

namespace Randomizer.Tools
{
    public static class Program
    {
        private const char SoftHyphen = (char)0xAD;
        private static readonly string SoftHyphens = new(SoftHyphen, 1);
        private static readonly Hyphenator s_hyphenator;

        static Program()
        {
            s_hyphenator = new Hyphenator(HyphenatePatternsLanguage.EnglishUs, 
                hyphenateSymbol: SoftHyphens,
                hyphenateLastWord: true);
        }

        public static void Main(string[] args)
        {
            var rootCommand = new RootCommand("SMZ3 Randomizer command-line tools");
            var formatText = new Command("format", "Formats text entries for ALttP.")
            {
                new Argument<FileInfo>("input",
                    "The file containing the entries to format. Every entry should be on a separate line.")
                    .ExistingOnly(),
            };
            formatText.Handler = CommandHandler.Create<FileInfo>(FormatText);
            rootCommand.AddCommand(formatText);
            rootCommand.Invoke(args);
        }

        public static void FormatText(FileInfo input)
        {
            if (!input.Exists)
                throw new FileNotFoundException($"Could not find the file '{input.FullName}'.");

            using var reader = new StreamReader(input.OpenRead());
            while (!reader.EndOfStream)
            {
                var text = reader.ReadLine();
                var hyphenatedText = s_hyphenator.HyphenateText(text);

                var result = string.Join('\n', FormatDialog(hyphenatedText));
                if (!Dialog.FitsSimple(result))
                    Console.WriteLine("<<<DOES NOT FIT SIMPLE>>>");
                if (!Dialog.FitsCompiled(result))
                    Console.WriteLine("<<<DOES NOT FIT COMPILED>>>");
                Console.WriteLine(result);
                Console.WriteLine("---");
            }
        }

        public static IEnumerable<string> FormatDialog(ReadOnlySpan<char> text, int wrap = 19)
        {
            var result = new List<string>();

            var cursor = 0;
            while (cursor < text.Length)
            {
                // Return remainder if there is not enough text left
                if (text[cursor..].Length < wrap)
                {
                    result.Add(text[cursor..].ToString().Replace(SoftHyphens, null));
                    break;
                }

                // Find the next part that fits within the line length
                var pos = text.LastIndexOfConvenientWrappingPoint(cursor, wrap);
                if (pos < 0)
                {
                    throw new ArgumentException($"The specified text cannot be broken up into parts of {wrap} characters or less. Text:\n{text.ToString()}");
                }

                var slice = text[cursor..pos].ToString().Replace(SoftHyphens, null); // Remove remaining soft hyphens
                if (text[pos] == SoftHyphen)
                    slice += '-'; // Turn the soft hyphen into a hard hyphen if we wrapped at one
                result.Add(slice);
                cursor = pos + 1; // Eat the space or hyphen
            }

            return result;
        }

        /// <summary>
        /// Returns the index of the last character in the string at which text should be wrapped.
        /// </summary>
        /// <param name="span">The span to search through.</param>
        /// <param name="start">The index at which to begin searching.</param>
        /// <param name="maxLength">The maximum number of characters to search through (excluding soft hyphens).</param>
        /// <returns>The index of the last character near the wrapping point, or -1.</returns>
        public static int LastIndexOfConvenientWrappingPoint(this ReadOnlySpan<char> span, int start, int maxLength)
        {
            var softHyphens = span.Slice(start, maxLength).ToArray().Count(x => x == SoftHyphen);
            var slice = start + maxLength + softHyphens > span.Length 
                ? span.Slice(start) 
                : span.Slice(start, maxLength + softHyphens);
            return start + slice.LastIndexOfAny(' ', SoftHyphen);
        }
    }
}