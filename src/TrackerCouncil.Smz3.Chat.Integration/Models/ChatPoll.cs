using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TrackerCouncil.Smz3.Chat.Integration.Models;

public class ChatPoll
{
    public bool IsPollComplete { get; set; }
    public bool IsPollSuccessful { get; set; }
    public bool WasPollTerminated { get; set; }
    public string? WinningChoice { get; set; }
}
