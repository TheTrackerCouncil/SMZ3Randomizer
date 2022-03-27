using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Randomizer.SMZ3.ChatIntegration;

namespace Randomizer.SMZ3.Twitch
{
    public class TwitchChatMessage : ChatMessage
    {
        public TwitchChatMessage(TwitchLib.Client.Models.ChatMessage message)
            : base(sender: message.DisplayName,
                   userName: message.Username,
                   text: message.Message)
        {

        }
    }
}
