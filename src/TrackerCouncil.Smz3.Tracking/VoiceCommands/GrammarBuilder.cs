using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Speech.Recognition;
using SharpYaml.Tokens;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

public enum GrammarElementType
{
    Rule,
    String,
    OneOf,
    Optional,
    KeyValue,
    GrammarElementList
}

public class GrammarValue(string key, object? value = null)
{
    public string Key { get; set; } = key;
    public object Value { get; set; } = value ?? key;
}
public class GrammarElements
{
    public bool IsOptional { get; set; }
    public GrammarElement[] Elements { get; set; }
}

public class GrammarElement
{
    public GrammarElementType Type { get; set; }
    public string? Key { get; set; }
    public object Data { get; set; } = string.Empty;
}

/// <summary>
/// Constructs a speech recognition grammar.
/// </summary>
public class GrammarBuilder
{
    // public static string[] ForceCommandIdentifiers = ["would you please", "please, I'm begging you"];
    //
    // private readonly System.Speech.Recognition.GrammarBuilder _grammar = null!;
    // private readonly List<string> _elements;
    // public readonly GrammarElement GrammarElements = new GrammarElements();
    //
    // /// <summary>
    // /// Initializes a new empty instance of the <see cref="GrammarBuilder"/>
    // /// class.
    // /// </summary>
    // public GrammarBuilder()
    // {
    //     if (OperatingSystem.IsWindows())
    //     {
    //         _grammar = new();
    //     }
    //     _elements = new();
    // }
    //
    // /// <summary>
    // /// Initializes a new instance of the <see cref="GrammarBuilder"/> class
    // /// that combines the specified grammars into a single choice.
    // /// </summary>
    // /// <param name="choices">The grammars to choose from.</param>
    // public GrammarBuilder(IEnumerable<GrammarBuilder> choices)
    //     : this()
    // {
    //     if (!OperatingSystem.IsWindows())
    //     {
    //         return;
    //     }
    //     _grammar.Append(new Choices(choices.Select(x => (System.Speech.Recognition.GrammarBuilder)x).ToArray()));
    //     var subElements = new List<GrammarElement>();
    //     foreach (var builder in choices)
    //     {
    //         subElements.Add(new GrammarElement()
    //         {
    //             Type = GrammarElementType.Rule,
    //             Data = builder.GrammarElements
    //         });
    //     }
    //     GrammarElements.Add(new GrammarElement()
    //     {
    //         Type = GrammarElementType.GrammarElementList,
    //         Data = subElements
    //     });
    //
    //     // _elements.Add(choice + "\n");
    // }
    //
    // public System.Speech.Recognition.GrammarBuilder ToNativeGrammar()
    // {
    //     System.Speech.Recognition.GrammarBuilder grammar = new System.Speech.Recognition.GrammarBuilder();
    //
    //     foreach (var element in GrammarElements)
    //     {
    //         if (element.)
    //     }
    // }
    //
    // /// <summary>
    // /// Converts the grammar builder to the System.Speech grammar builder.
    // /// </summary>
    // /// <param name="self">The grammar builder to convert.</param>
    // public static implicit operator System.Speech.Recognition.GrammarBuilder(GrammarBuilder self) => self._grammar;
    //
    // /// <summary>
    // /// Combines the specified grammars into a single grammar.
    // /// </summary>
    // /// <param name="choices">
    // /// The possible grammars to choose from in the new grammar.
    // /// </param>
    // /// <returns>
    // /// A new <see cref="GrammarBuilder"/> that represents a choice of one
    // /// of <paramref name="choices"/>.
    // /// </returns>
    // public static GrammarBuilder Combine(params GrammarBuilder[] choices)
    // {
    //     return new GrammarBuilder(choices);
    // }
    //
    // /// <summary>
    // /// Adds the specified text to the end of the grammar.
    // /// </summary>
    // /// <param name="text">The text to recognize.</param>
    // /// <returns>This instance.</returns>
    // public GrammarBuilder Append(string text)
    // {
    //     if (!OperatingSystem.IsWindows())
    //     {
    //         return this;
    //     }
    //     _grammar.Append(text);
    //     _elements.Add(text);
    //     GrammarElements.Add(new GrammarElement()
    //     {
    //         Type = GrammarElementType.String,
    //         Data = text
    //     });
    //     return this;
    // }
    //
    // /// <summary>
    // /// Adds a choice that can be retrieved later using the specified
    // /// semantic result key.
    // /// </summary>
    // /// <param name="key">
    // /// The key used to retrieve the recognized choice.
    // /// </param>
    // /// <param name="choices">
    // /// The choices to represent in the grammar.
    // /// </param>
    // /// <returns>This instance.</returns>
    // public GrammarBuilder Append(string key, List<GrammarValue> choices)
    // {
    //     if (!OperatingSystem.IsWindows())
    //     {
    //         return this;
    //     }
    //
    //     var choices2 = new Choices();
    //     foreach (var choice in choices)
    //     {
    //         choices2.Add(new SemanticResultValue(choice.Key, choice.Value));
    //     }
    //
    //     _grammar.Append(new SemanticResultKey(key, choices2));
    //     _elements.Add($"<{key}>");
    //     GrammarElements.Add(new GrammarElement()
    //     {
    //         Type = GrammarElementType.KeyValue,
    //         Data = choices.ToArray(),
    //         Key = key
    //     });
    //     return this;
    // }
    //
    // /// <summary>
    // /// Adds a choice.
    // /// </summary>
    // /// <param name="choices">
    // /// The choices to represent in the grammar.
    // /// </param>
    // /// <returns>This instance.</returns>
    // public GrammarBuilder OneOf(params string[] choices)
    // {
    //     if (!OperatingSystem.IsWindows())
    //     {
    //         return this;
    //     }
    //     _grammar.Append(new Choices(choices));
    //     _elements.Add($"[{string.Join('/', choices)}]");
    //     GrammarElements.Add(new GrammarElement()
    //     {
    //         Type = GrammarElementType.OneOf,
    //         Data = choices
    //     });
    //     return this;
    // }
    //
    // /// <summary>
    // /// Adds the specified optional text to the end of the grammar.
    // /// </summary>
    // /// <param name="text">
    // /// The text that may or may not be recognized.
    // /// </param>
    // /// <returns>This instance.</returns>
    // public GrammarBuilder Optional(string text)
    // {
    //     if (!OperatingSystem.IsWindows())
    //     {
    //         return this;
    //     }
    //     _grammar.Append(text, 0, 1);
    //     _elements.Add($"({text})");
    //     string[] choices = [ text ];
    //     GrammarElements.Add(new GrammarElement()
    //     {
    //         Type = GrammarElementType.Optional,
    //         Data = choices
    //     });
    //     return this;
    // }
    //
    // /// <summary>
    // /// Adds an optional choice.
    // /// </summary>
    // /// <param name="choices">The choices to represent in the grammar.</param>
    // /// <returns>This instance.</returns>
    // public GrammarBuilder Optional(params string[] choices)
    // {
    //     if (!OperatingSystem.IsWindows())
    //     {
    //         return this;
    //     }
    //     _grammar.Append(new Choices(choices), 0, 1);
    //     _elements.Add($"({string.Join('/', choices)})");
    //     GrammarElements.Add(new GrammarElement()
    //     {
    //         Type = GrammarElementType.Optional,
    //         Data = choices
    //     });
    //     return this;
    // }
    //
    // /// <summary>
    // /// Builds the grammar.
    // /// </summary>
    // /// <param name="name">The name of the grammar rule.</param>
    // /// <returns>A new <see cref="Grammar"/>.</returns>
    // [SupportedOSPlatform("windows")]
    // public Grammar Build(string name)
    // {
    //     return new Grammar(this)
    //     {
    //         Name = name
    //     };
    // }
    //
    // public List<string> GeneratePermutations()
    // {
    //     List<string[]> stringArrays = [];
    //
    //     foreach (var element in _elements)
    //     {
    //         if (element.StartsWith("("))
    //         {
    //             stringArrays.Add(element.Substring(1, element.Length - 2).Split("/").Concat([""]).ToArray());
    //         }
    //         else if (element.StartsWith("["))
    //         {
    //             stringArrays.Add(element.Substring(1, element.Length - 2).Split("/"));
    //         }
    //         else
    //         {
    //             stringArrays.Add([element]);
    //         }
    //     }
    //
    //     // Base case: if no arrays, return an empty list
    //     if (stringArrays == null || stringArrays.Count == 0)
    //     {
    //         return new List<string>();
    //     }
    //
    //     // Start with an initial list containing an empty string
    //     List<string> results = new List<string> { "" };
    //
    //     // Iterate through each array in the input list
    //     foreach (var array in stringArrays)
    //     {
    //         // Create a new list to store the updated combinations
    //         List<string> newResults = new List<string>();
    //
    //         // Append each element of the current array to each existing combination
    //         foreach (var result in results)
    //         {
    //             foreach (var item in array)
    //             {
    //                 newResults.Add(string.IsNullOrEmpty(result) ? item : result + " " + item);
    //             }
    //         }
    //
    //         // Update results with the new combinations
    //         results = newResults;
    //     }
    //
    //     return results.Select(x => "\"" + x + "\"").ToList();
    // }
    //
    // /// <summary>
    // /// Returns a string representing the grammar syntax.
    // /// </summary>
    // /// <returns>A string representing the grammar syntax.</returns>
    // public override string ToString()
    //     => string.Join(' ', _elements);
}
