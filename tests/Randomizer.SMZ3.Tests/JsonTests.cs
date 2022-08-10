using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Newtonsoft.Json.Linq;
using Randomizer.SMZ3.Tracking.Configuration;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using SharpYaml.Tokens;

using Xunit;

namespace Randomizer.SMZ3.Tests
{
    public class JsonTests
    {
        /*
         * [Fact]
        public void TrackerConfigDoesNotContainDebugWeights()
        {
            var provider = new TrackerConfigProvider(null);
            var config = provider.GetTrackerConfig();

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void LocationConfigDoesNotContainDebugWeights()
        {
            var provider = new TrackerConfigProvider(null);
            var config = provider.GetLocationConfig();

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void TrackerConfigDoesNotContainCurlyQuotes()
        {
            var provider = new TrackerConfigProvider(null);
            var config = provider.GetTrackerConfig();

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Text.Contains('’'), because: "curly quotes can mess up text-to-speech on some systems");
            }
        }
        */
        private IEnumerable<SchrodingersString> EnumerateSchrodingersStrings(object obj, int depth = 0)
        {
            if (obj is null)
            {
                yield break;
            }
            else if (obj is SchrodingersString str)
            {
                yield return str;
            }
            else if (obj is IEnumerable<SchrodingersString> strings)
            {
                foreach (var x in strings)
                    yield return x;
            }
            else
            {
                if (depth > 3) yield break;

                if (obj is IEnumerable enumerable)
                {
                    foreach (var nestedObj in enumerable)
                    {
                        var nestedStrings = EnumerateSchrodingersStrings(nestedObj, depth + 1);
                        foreach (var x in nestedStrings)
                            yield return x;
                    }
                }
                else
                {
                    foreach (var property in obj.GetType().GetProperties())
                    {
                        var value = property.GetValue(obj);
                        var nestedStrings = EnumerateSchrodingersStrings(value, depth + 1);
                        foreach (var x in nestedStrings)
                            yield return x;
                    }
                }
            }
        }
    }
}
