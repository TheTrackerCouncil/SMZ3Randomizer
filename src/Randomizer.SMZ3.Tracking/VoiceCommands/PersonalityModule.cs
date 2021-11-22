using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Extensions.Logging;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Provides voice commands relating to Tracker's personality. 
    /// </summary>
    public class PersonalityModule : TrackerModule
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PersonalityModule"/> class.
        /// </summary>
        /// <param name="tracker">The tracker instance to use.</param>
        /// <param name="logger">Used to write logging information.</param>
        public PersonalityModule(Tracker tracker, ILogger<PersonalityModule> logger)
            : base(tracker, logger)
        {
            AddCommand("Ask about tracker's mood", GetMoodRule(), (tracker, result) =>
            {
                tracker.Say(tracker.Responses.Moods[tracker.Mood]);
            });
        }

        /// <summary>
        /// Gets a value indicating whether the voice recognition syntax is
        /// secret and should not be displayed to the user.
        /// </summary>
        public override bool IsSecret => true;

        private GrammarBuilder GetMoodRule()
        {
            return new GrammarBuilder()
                .Append("Hey tracker,")
                .OneOf("how are you?", "how are you doing?", "how are you feeling?");
        }
    }
}
