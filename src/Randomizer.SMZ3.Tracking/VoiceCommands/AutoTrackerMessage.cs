using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    public class AutoTrackerMessage
    {
        public string Action { get; set; }
        public int Address { get; set; }
        public int Length { get; set; }
        public int[] Bytes { get; set; }
        public Game Game { get; set; } = Game.Both;

        public bool ShouldSend(Game currentGame, bool hasStartedGame)
        {
            return (!hasStartedGame && Game == Game.Neither) || (hasStartedGame && Game != Game.Neither && (Game == Game.Both || Game == currentGame));
        }
    }

    public enum Game
    {
        Neither,
        SM,
        Zelda,
        Both
    }
}
