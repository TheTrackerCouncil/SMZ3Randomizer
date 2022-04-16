using System;

namespace Randomizer.SMZ3
{
    /// <summary>
    /// Class for housing potentially expected exceptions when generating seeds
    /// </summary>
    public class RandomizerGenerationException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizerGenerationException"/>
        /// class.
        /// </summary>
        public RandomizerGenerationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizerGenerationException"/>
        /// class with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public RandomizerGenerationException(string message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RandomizerGenerationException"/>
        /// class with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">The exception that is the cause of the current exception.</param>
        public RandomizerGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
