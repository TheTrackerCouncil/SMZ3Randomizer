using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

using FluentAssertions;

using Newtonsoft.Json.Linq;
using Randomizer.Data.Configuration;
using Randomizer.Data.Configuration.ConfigFiles;
using Randomizer.Data.Configuration.ConfigTypes;
using SharpYaml.Tokens;

using Xunit;

namespace Randomizer.SMZ3.Tests
{
    public class ConfigFileTests
    {
        [Fact]
        public void BossConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetBossConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void DungeonConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetDungeonConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void ItemConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetItemConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void LocationConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetLocationConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void RegionConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetRegionConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void RequestConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetRequestConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void ResponseConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetResponseConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void RewardConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetRewardConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void RoomConfigDoesNotContainDebugWeights()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetRoomConfig("BCU", "Sassy");

            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        [Fact]
        public void GameLinesConfigHasValidStrings()
        {
            var provider = new ConfigProvider(null);
            var config = provider.GetGameConfig("BCU", "Sassy");
            var strings = EnumerateSchrodingersStrings(config);
            foreach (var value in strings)
            {
                value.Should().NotContain(x => x.Weight >= 90);
            }
        }

        private bool HasLongLine(SchrodingersString.Possibility possibility)
        {
            return possibility.Text.Split("\n").Any(x => x.Length > 19);
        }

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
                        if (property.PropertyType == typeof(Type))
                            continue;
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
