using System;
using System.Linq;

using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Debug;

using Xunit;

namespace Randomizer.SMZ3.Msu.Tests
{
    public class MsuPackFactoryTests
    {
        private readonly LoggerFactory _loggerFactory;

        public MsuPackFactoryTests()
        {
            _loggerFactory = new LoggerFactory();
            _loggerFactory.AddProvider(new DebugLoggerProvider());
        }

        [Fact]
        public void AutoDetectMusicPacks()
        {
            var factory = new MusicPackFactory(Logger<MusicPackFactory>());
            var packs = factory.AutoDetectAll(@"C:\Users\laura\Documents\SMZ3 Cas Randomizer\MSU packs")
                .ToList();
        }

        [Fact]
        public void AutoDetectMusicPack()
        {
            var factory = new MusicPackFactory(Logger<MusicPackFactory>());
            var pack = factory.AutoDetect(@"C:\Users\laura\Documents\SMZ3 Cas Randomizer\MSU packs\Follin\Follin_SMZ3.msu");
        }

        private ILogger<T> Logger<T>() => _loggerFactory.CreateLogger<T>();
    }
}
