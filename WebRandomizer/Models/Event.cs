﻿using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace WebRandomizer.Models {
    public enum EventType {
        ItemReceived,       // Item received from player
        ItemSent,           // Item sent to player
        ItemFound,          // Item found by player (for themselves)
        Multiworld          // General multiworld events (connections, spoiler viewing etc)
    }
    
    public class Event {
        public int Id { get; set; }

        public int ClientId { get; set; }

        public EventType Type { get; set; }
        
        public int SequenceNum { get; set; }
        
        public int PlayerId { get; set; }
        
        public int ItemId { get; set; }
        
        public DateTime TimeStamp { get; set; }
        
        public string Description { get; set; }
    }
}
