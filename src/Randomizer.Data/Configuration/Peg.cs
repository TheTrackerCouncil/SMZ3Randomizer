using System;
using System.Text.Json.Serialization;

using Randomizer.Data.Configuration.Converters;

namespace Randomizer.Data.Configuration
{
    /// <summary>
    /// Represents a peg in Peg World mode.
    /// </summary>
    [JsonConverter(typeof(PegConverter))]
    public class Peg
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Peg"/> class.
        /// </summary>
        /// <param name="column">The zero-based index of the column.</param>
        /// <param name="row">The zero-based index of the row.</param>
        public Peg(int column, int row)
        {
            Column = column;
            Row = row;
        }

        /// <summary>
        /// Gets the zero-based index of the column in which the peg is shown.
        /// </summary>
        public int Column { get; }

        /// <summary>
        /// Gets the zero-based index of the row in which the peg is shown.
        /// </summary>
        public int Row { get; }

        /// <summary>
        /// Indicates whether the peg has been pegged.
        /// </summary>
        public bool Pegged { get; set; }
    }
}
