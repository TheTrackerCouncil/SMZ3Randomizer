using System;

namespace Randomizer.SMZ3.Tracking.VoiceCommands;

/// <summary>
/// Represents errors that occur when constructing a speech recognition
/// grammar.
/// </summary>
/// <param name="message">Exception message</param>
/// <param name="innerException">Original exception</param>
public class GrammarException(string? message, Exception? innerException) : Exception(message, innerException);
