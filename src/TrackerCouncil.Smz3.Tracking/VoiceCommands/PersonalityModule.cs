﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;
using TrackerCouncil.Smz3.Tracking.Services;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Provides voice commands relating to Tracker's personality.
/// </summary>
public class PersonalityModule : TrackerModule
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PersonalityModule"/> class.
    /// </summary>
    /// <param name="tracker">The tracker instance to use.</param>
    /// <param name="playerProgressionService">Service to get item information</param>
    /// <param name="worldQueryService">Service to get world information</param>
    /// <param name="logger">Used to write logging information.</param>
    public PersonalityModule(TrackerBase tracker, IPlayerProgressionService playerProgressionService, IWorldQueryService worldQueryService, ILogger<PersonalityModule> logger)
        : base(tracker, playerProgressionService, worldQueryService, logger)
    {

    }

    public override void AddCommands()
    {
        AddCommand("Hey, ya missed pal", GetYaMissedRule(), (_) =>
        {
            TrackerBase.Say(text: "Here Mike. This will explain everything.", wait: true);
            OpenInBrowser(new Uri("https://www.youtube.com/watch?v=5P6UirFDdxM"));
        });

        foreach (var request in TrackerBase.Requests)
        {
            if (request.Phrases.Count == 0)
                continue;

            AddCommand(request.Phrases.First(), GetRequestRule(request.Phrases), (_) =>
            {
                TrackerBase.Say(response: request.Response);
            });
        }
    }

    /// <summary>
    /// Gets a value indicating whether the voice recognition syntax is
    /// secret and should not be displayed to the user.
    /// </summary>
    public override bool IsSecret => true;

    private SpeechRecognitionGrammarBuilder GetYaMissedRule()
    {
        return new SpeechRecognitionGrammarBuilder()
            .Append("Hey tracker,")
            .Append("what do you know about")
            .OneOf("your missing steak knives?", "my shoes getting bloody?");
    }

    private SpeechRecognitionGrammarBuilder GetRequestRule(IEnumerable<string> requests)
    {
        return new SpeechRecognitionGrammarBuilder(requests.Select(x =>
        {
            return new SpeechRecognitionGrammarBuilder()
                .Append("Hey tracker, ")
                .Append(x);
        }));
    }

    private static void OpenInBrowser(Uri address)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = address.ToString(),
            UseShellExecute= true
        };
        Process.Start(startInfo);
    }
}
