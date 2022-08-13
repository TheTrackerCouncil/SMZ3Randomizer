using System;
using System.Collections.Generic;
using Randomizer.Shared;
using static Randomizer.SMZ3.Tracking.Configuration.ConfigTypes.SchrodingersString;
using Randomizer.SMZ3.Tracking.Configuration.ConfigTypes;
using System.Reflection;

namespace Randomizer.SMZ3.Tracking.Configuration.ConfigFiles
{
    /// <summary>
    /// Config file for additional request information
    /// </summary>
    public class RequestConfig : List<BasicVoiceRequest>, IMergeable<BasicVoiceRequest>, IConfigFile<RequestConfig>
    {
        /// <summary>
        /// Constructor
        /// </summary>
        public RequestConfig() : base()
        {
        }

        /// <summary>
        /// Returns default request information
        /// </summary>
        /// <returns></returns>
        public static RequestConfig Default()
        {
            return new RequestConfig
            {
                new BasicVoiceRequest()
                {
                    Phrases = new() { "how do I crystal flash?", "how do I perform a crystal flash?", "how does crystal flash work?", "how can I recover energy in Super Metroid?" },
                    Response = new("To perform a crystal flash, you must have 50 or less energy and at least 10 missiles, super missiles and 11 power bombs. Then, enter Morph Ball form and use a power bomb and hold both aim up, aim down, down and fire buttons."),
                },
                new BasicVoiceRequest()
                {
                    Phrases = new() { "how do I use special beams?", "how do I use special charge beam attacks?" },
                    Response = new("If you have the Charge Beam and one other beam currently active, you can select Power Bombs and charge up a beam to do a special charge beam attack."),
                },
                new BasicVoiceRequest()
                {
                    Phrases = new() { "how do I shinespark?", "how does shine spark work?" },
                    Response = new("Run with Speed Booster and crouch while still holding a direction. It'll hurt though."),
                },
                new BasicVoiceRequest()
                {
                    Phrases = new() { "do you know any special tricks?" },
                    Response = new("Ask me how to use special charge beam attacks.", "Ask me how to perform a crystal flash.", "You can drop five bombs if you enter Morph Ball form while charging a beam."),
                },
            };
        }
    }
}
