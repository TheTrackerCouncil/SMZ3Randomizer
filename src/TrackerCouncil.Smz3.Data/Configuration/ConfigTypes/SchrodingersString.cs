﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Text.Json.Serialization;
using TrackerCouncil.Smz3.Data.Configuration.Converters;

namespace TrackerCouncil.Smz3.Data.Configuration.ConfigTypes;

/// <summary>
/// Represents multiple possibilities of a string.
/// </summary>
[JsonConverter(typeof(SchrodingersStringConverter))]
[DebuggerDisplay("[{GetDebuggerDisplay()}]")]
public class SchrodingersString : Collection<SchrodingersString.Possibility>, IMergeable<SchrodingersString>
{
    private static readonly Random s_random = new();

    /// <summary>
    /// Initializes a new instance of the <see cref="SchrodingersString"/>
    /// class that is empty.
    /// </summary>
    public SchrodingersString()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchrodingersString"/>
    /// class with the specified items.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public SchrodingersString(IEnumerable<Possibility> items)
    {
        foreach (var item in items)
            base.Add(item);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchrodingersString"/>
    /// class with the specified items using default weights.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public SchrodingersString(IEnumerable<string> items)
    {
        foreach (var item in items)
            Add(item);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="SchrodingersString"/>
    /// class with the specified items.
    /// </summary>
    /// <param name="items">The items to add.</param>
    public SchrodingersString(params Possibility[] items)
    {
        foreach (var item in items)
            base.Add(item);
    }

    /// <summary>
    /// Returns a random string from the specified possibilities.
    /// </summary>
    /// <param name="value"></param>
    public static implicit operator string?(SchrodingersString? value)
        => value?.ToString();

    /// <summary>
    /// Determines whether the specified text is among the possibilities for
    /// this string.
    /// </summary>
    /// <param name="text">The string to test.</param>
    /// <param name="stringComparison">
    /// The type of comparison method to use.
    /// </param>
    /// <returns>
    /// <see langword="true"/> if <paramref name="text"/> is among the
    /// possibilities, otherwise, <see langword="false"/>.
    /// </returns>
    public bool Contains(string text, StringComparison stringComparison)
    {
        return Items.Any(x => x.Text.Equals(text, stringComparison));
    }

    /// <summary>
    /// Returns a random string from the possibilities.
    /// </summary>
    /// <returns>A random string from the possibilities.</returns>
    public override string? ToString() => Random(s_random)?.Text;

    /// <summary>
    /// Adds a new possibility to the string with the specified text and
    /// weight.
    /// </summary>
    /// <param name="text">The possibility to add.</param>
    /// <param name="weight">The weight of the possibility to add.</param>
    public void Add(string text, double weight = 1.0d)
    {
        base.Add(new Possibility(text, weight));
    }

    /// <summary>
    /// Gets the details needed for tracker speaking
    /// </summary>
    /// <param name="args"></param>
    /// <returns>The text for tracker to speak and the image to display, if applicable</returns>
    public (string SpeechText, string? TrackerImage, List<PossibilityAdditionalLine>? AdditionalLines)? GetSpeechDetails(params object?[] args)
    {
        var selectedPossibility = Random(s_random);
        if (selectedPossibility == null)
        {
            return null;
        }
        var formattedText = string.Format(selectedPossibility.Text, args);
        return (formattedText, selectedPossibility.TrackerImage, selectedPossibility.AdditionalLines);
    }

    /// <summary>
    /// Replaces placeholders in the string with the specified values.
    /// </summary>
    /// <param name="args">
    /// A collection of objects to format the string with.
    /// </param>
    /// <returns>The formatted string, or <c>null</c> if</returns>
    public string? Format(params object?[] args)
    {
        var value = ToString();
        return value != null ? string.Format(value, args) : null;
    }

    /// <summary>
    /// Picks a phrase at random, taking into account any weights associated
    /// with the phrases.
    /// </summary>
    /// <param name="random">The random number generator to use.</param>
    /// <returns>
    /// A random phrase from the phrase set, or <c>null</c> if the phrase
    /// set contains no items.
    /// </returns>
    protected Possibility? Random(Random random)
    {
        if (Items.Count == 0)
            return null;

        var totalWeight = GetTotalWeight();

        if (totalWeight >= 1000)
        {
            var item = Items.FirstOrDefault(x => x.Weight >= 1000);
            if (item != null)
            {
                return item;
            }
        }

        var target = random.NextDouble() * totalWeight;
        foreach (var item in Items)
        {
            if (target < item.Weight)
                return item;

            target -= item.Weight;
        }

        throw new Exception("This code should not be reachable.");
    }

    private double GetTotalWeight() => Items.Sum(x => x.Weight);

    private string GetDebuggerDisplay() => string.Join(", ", this.Select(x => x.ToString()));

    public void MergeFrom(IMergeable<SchrodingersString> other)
    {
        if (other is SchrodingersString otherObj)
        {
            foreach (var possibility in otherObj.Where(x => !Contains(x)))
            {
                base.Add(possibility);
            }
        }
    }

    /// <summary>
    /// Represents one possibility of a <see cref="SchrodingersString"/>.
    /// </summary>
    [JsonConverter(typeof(SchrodingersStringItemConverter))]
    [DebuggerDisplay("{Text} Weight = {Weight}")]
    public class Possibility
    {
        /// <summary>
        /// The weight of items that do not have an explicit weight assigned
        /// to them.
        /// </summary>
        public const double DefaultWeight = 1.0d;

        public Possibility()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Possibility"/>
        /// class with the specified text and the default weight of 1.
        /// </summary>
        /// <param name="text">The text.</param>
        public Possibility(string text) : this(text, DefaultWeight)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Possibility"/>
        /// class with the specified text and weight.
        /// </summary>
        /// <param name="text">The text.</param>
        /// <param name="weight">
        /// The weight for the item, based on a default of 1.
        /// </param>
        public Possibility(string text, double weight)
        {
            if (weight < 0)
                throw new ArgumentOutOfRangeException(nameof(weight), weight, "Weight cannot be less than zero.");

            Text = text;
            Weight = weight;
        }

        /// <summary>
        /// Gets a string.
        /// </summary>
        public string Text { get; init; } = "";

        /// <summary>
        /// Gets the weight associated with the item.
        /// </summary>
        [DefaultValue(DefaultWeight)]
        public double Weight { get; init; } = DefaultWeight;

        /// <summary>
        /// The tracker image to display
        /// </summary>
        public string? TrackerImage { get; init; } = null;

        /// <summary>
        /// Additional lines to be stated after the initial line. Used
        /// for adding additional poses.
        /// </summary>
        public List<PossibilityAdditionalLine>? AdditionalLines { get; set; }

        /// <summary>
        /// Converts the possibility to a string.
        /// </summary>
        /// <param name="item">The item to convert.</param>
        public static implicit operator string(Possibility item) => item.Text;

        /// <summary>
        /// Converts the text to a new possibility.
        /// </summary>
        /// <param name="text">The text to use.</param>
        public static implicit operator Possibility(string text) => new(text);

        /// <summary>
        /// Returns the hash code for this possibility.
        /// </summary>
        /// <returns>A 32-bit signed integer hash code.</returns>
        public override int GetHashCode() => Text.GetHashCode();

        /// <summary>
        /// Returns a string representation of the possibility.
        /// </summary>
        /// <returns>A string representation of the possibility.</returns>
        public override string ToString() => Text;

        /// <summary>
        /// Checks if one possibility is the same as another
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public override bool Equals(object? other)
        {
            if (other is Possibility otherPossibility)
            {
                return Text.Equals(otherPossibility.Text) && Math.Abs(Weight - otherPossibility.Weight) < .01;
            }
            return false;
        }
    }
}

public class PossibilityAdditionalLine
{
    public string Text { get; init; } = "";

    public string? TrackerImage { get; init; } = null;
}
