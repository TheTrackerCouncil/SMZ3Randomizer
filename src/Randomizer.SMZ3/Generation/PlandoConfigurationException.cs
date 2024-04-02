using System;

namespace Randomizer.SMZ3.Generation;

/// <summary>
/// Represents an error from attempting to create a plando from a YAML config
/// </summary>
/// <param name="message">Exception message</param>
public class PlandoConfigurationException(string message) : Exception(message);
