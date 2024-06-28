using TrackerCouncil.Smz3.Chat.Integration.Models;

namespace TrackerCouncil.Smz3.Chat.Twitch;

public class TwitchChatMessage(TwitchLib.Client.Models.ChatMessage message) : ChatMessage(sender: message.DisplayName,
    userName: message.Username,
    text: message.Message,
    isModerator: message.IsModerator || message.IsBroadcaster);
