using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Randomizer.App
{
    public static class EmbeddedResource
    {
        public static Stream? GetStream<T>(string resourceName)
        {
            var assembly = typeof(T).Assembly;
            return assembly.GetManifestResourceStream(typeof(T), resourceName);
        }
    }
}
