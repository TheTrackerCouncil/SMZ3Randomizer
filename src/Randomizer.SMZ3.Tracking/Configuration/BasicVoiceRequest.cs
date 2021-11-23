using System;
using System.Collections.Generic;

namespace Randomizer.SMZ3.Tracking.Configuration
{
    /// <summary>
    /// Represents a basic request that supports voice recognition with a simple
    /// response.
    /// </summary>
    public class BasicVoiceRequest
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="BasicVoiceRequest"/>
        /// class.
        /// </summary>
        /// <param name="phrases">The phrases that should be recognized.</param>
        /// <param name="response">The possible responses to the requests</param>
        public BasicVoiceRequest(IReadOnlyCollection<string> phrases, SchrodingersString response)
        {
            Phrases = phrases;
            Response = response;
        }

        /// <summary>
        /// Gets a collection of phrases that should be recognized.
        /// </summary>
        public IReadOnlyCollection<string> Phrases { get; }

        /// <summary>
        /// Gets the possible responses to the request.
        /// </summary>
        public SchrodingersString Response { get; }
    }
}
