using System;
using System.Runtime.Serialization;

namespace Randomizer.SMZ3.Tracking.VoiceCommands
{
    /// <summary>
    /// Represents errors that occur when constructing a speech recognition
    /// grammar.
    /// </summary>
    public class GrammarException : Exception
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarException"/>
        /// class.
        /// </summary>
        public GrammarException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarException"/>
        /// class with the specified message.
        /// </summary>
        /// <param name="message">The error message.</param>
        public GrammarException(string? message)
            : base(message)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarException"/>
        /// class with the specified message and inner error.
        /// </summary>
        /// <param name="message">The error message.</param>
        /// <param name="innerException">
        /// The exception that is the cause of the current exception.
        /// </param>
        public GrammarException(string? message, Exception? innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="GrammarException"/>
        /// with serialized data.
        /// </summary>
        /// <param name="info">
        /// The SerializationInfo that holds the serialized object data about
        /// the exception being thrown.
        /// </param>
        /// <param name="context">
        /// The StreamingContext that contains contextual information about the
        /// source or destination.
        /// </param>
        [Obsolete("Serialization constructors are deprecated in .NET8+", DiagnosticId = "SYSLIB0051")]
        protected GrammarException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
