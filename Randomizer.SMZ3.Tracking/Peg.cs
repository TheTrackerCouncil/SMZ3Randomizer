using System;
using System.Text.Json.Serialization;

using Randomizer.SMZ3.Tracking.Converters;

namespace Randomizer.SMZ3.Tracking
{
    /// <summary>
    /// Represents a peg in Peg World mode.
    /// </summary>
    [JsonConverter(typeof(PegConverter))]
    public class Peg
    {
        public Peg(int column, int row)
        {
            Column = column;
            Row = row;
        }

        public int Column { get; }

        public int Row { get; }

        public bool Pegged { get; set; }
    }
}
