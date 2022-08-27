using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.ExceptionServices;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.SMZ3.Generation
{
    public class PlandoConfigurationException : Exception
    {
        public PlandoConfigurationException()
        {
        }

        public PlandoConfigurationException(string message) : base(message)
        {
        }

        public PlandoConfigurationException(string message, Exception innerException) : base(message, innerException)
        {
        }

        protected PlandoConfigurationException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
