using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.SMZ3.ChatIntegration.Models;

namespace Randomizer.SMZ3.ChatIntegration
{
    public class MessageReceivedEventArgs : EventArgs
    {
        public MessageReceivedEventArgs(ChatMessage message)
        {
            Message = message;
        }

        public ChatMessage Message { get; }
    }
}
