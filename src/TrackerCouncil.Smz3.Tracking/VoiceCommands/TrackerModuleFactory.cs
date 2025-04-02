using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using PySpeechService.Recognition;
using TrackerCouncil.Smz3.Abstractions;

namespace TrackerCouncil.Smz3.Tracking.VoiceCommands;

/// <summary>
/// Loads and manages tracker modules.
/// </summary>
public class TrackerModuleFactory : IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private IEnumerable<TrackerModule>? _trackerModules;
    private ILogger<TrackerModuleFactory> _logger;
    private Dictionary<string, IEnumerable<string>> _syntax = [];

    /// <summary>
    /// Initializes a new instance of the <see cref="TrackerModuleFactory"/>
    /// class.
    /// </summary>
    /// <param name="serviceProvider">
    /// Used to load available tracker modules.
    /// </param>
    /// <param name="logger"></param>
    public TrackerModuleFactory(IServiceProvider serviceProvider, ILogger<TrackerModuleFactory> logger)
    {
        _serviceProvider = serviceProvider;
        _logger = logger;
    }

    /// <summary>
    /// Loads all available tracker modules into the specified speech
    /// recognition engine.
    /// </summary>
    /// <param name="tracker">The tracker instance.</param>
    /// <param name="moduleLoadError"></param>
    /// <returns>
    /// A dictionary that contains the loaded speech recognition syntax.
    /// </returns>
    public List<SpeechRecognitionGrammar> RetrieveGrammar(TrackerBase tracker, out bool moduleLoadError)
    {
        moduleLoadError = false;
        _trackerModules = _serviceProvider.GetServices<TrackerModule>().ToList();

        var grammar = new List<SpeechRecognitionGrammar>();

        foreach (var module in _trackerModules)
        {
            try
            {
                module.AddCommands();
                module.LoadInto(grammar);
                foreach (var syntax in module.Syntax)
                {
                    _syntax.Add(syntax.Key, syntax.Value);
                }
            }
            catch (InvalidOperationException e)
            {
                _logger.LogError(e, $"Error with loading module {module.GetType().Name}");
                moduleLoadError = true;
            }
        }

        return grammar;
    }

    public Dictionary<string, IEnumerable<string>> Syntax => _syntax;

    /// <summary>
    /// Retrieves the created module of the particular type
    /// </summary>
    /// <typeparam name="T">The type of module to retrieve</typeparam>
    /// <returns></returns>
    public T? GetModule<T>() where T : TrackerModule
    {
        return (T?)_trackerModules?.FirstOrDefault(x => x.GetType() == typeof(T));
    }

    /// <summary>
    /// Frees up resources used by tracker modules.
    /// </summary>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Frees up resources used by tracker modules.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose()"/>;
    /// otherwise, <see langword="false"/>.
    /// </param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing)
        {
            if (_trackerModules != null)
            {
                foreach (var module in _trackerModules)
                {
                    if (module is IDisposable disposable)
                        disposable.Dispose();
                }
            }
        }
    }
}
