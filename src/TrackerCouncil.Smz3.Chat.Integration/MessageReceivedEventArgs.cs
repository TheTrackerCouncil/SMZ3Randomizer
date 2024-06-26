using System;
using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Integration;

public class MessageReceivedEventArgs : EventArgs
{
    public MessageReceivedEventArgs(ChatMessage message)
    {
        Message = message;
    }

    public ChatMessage Message { get; }
}
