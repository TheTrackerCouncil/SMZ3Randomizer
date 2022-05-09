using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.ChatIntegration.Models
{
    public abstract class ChatMessage
    {
        protected ChatMessage(string sender, string userName, string text, bool isModerator)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            SenderUserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Text = text ?? throw new ArgumentNullException(nameof(text));
            IsFromModerator = isModerator;
        }

        public virtual string Sender { get; protected init; }

        public virtual string SenderUserName { get; protected init; }

        public virtual string Text { get; protected init; }

        public virtual bool IsFromModerator { get; protected init; }
    }
}
