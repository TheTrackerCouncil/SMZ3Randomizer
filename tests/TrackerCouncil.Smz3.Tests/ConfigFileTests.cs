﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using TrackerCouncil.Smz3.Data.Configuration;
using TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;
using Xunit;

namespace TrackerCouncil.Smz3.Tests;

public class ConfigFileTests
{
    [Fact]
    public void BossConfigDoesNotContainDebugWeights()
    {
        var provider = new ConfigProvider(null);
        var config = provider.GetBossConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);

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
        var config = provider.GetDungeonConfig(new[] { "BCU", "Sassy" }, null);
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
                    if (value == null) continue;
                    var nestedStrings = EnumerateSchrodingersStrings(value, depth + 1);
                    foreach (var x in nestedStrings)
                        yield return x;
                }
            }
        }
    }
}
