using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.ChatIntegration
{
    public abstract class ChatMessage
    {
        protected ChatMessage(string sender, string userName, string text)
        {
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            SenderUserName = userName ?? throw new ArgumentNullException(nameof(userName));
            Text = text ?? throw new ArgumentNullException(nameof(text));
        }

        public virtual string Sender { get; protected init; }

        public virtual string SenderUserName { get; protected init; }

        public virtual string Text { get; protected init; }
    }
}
