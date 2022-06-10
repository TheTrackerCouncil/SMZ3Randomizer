using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.ChatIntegration.Models
{
    /// <summary>
    /// Base class to be extended for TwitchAPI responses
    /// </summary>
    public class TwitchAPIResponse
    {
        /// <summary>
        /// If the Twitch API returned a successful response and
        /// was able to be parsed successfully
        /// </summary>
        public bool IsSuccessful { get; set; }
    }
}
