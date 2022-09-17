using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Randomizer.Data.WorldData;
using Randomizer.Data.WorldData.Regions;
using Randomizer.Shared.Models;

namespace Randomizer.Data.Services
{
    public class TrackerStateService : ITrackerStateService
    {
        private readonly RandomizerContext _randomizerContext;

        public TrackerStateService(RandomizerContext dbContext)
        {
            _randomizerContext = dbContext;
        }

        public void CreateState(World world, GeneratedRom generatedRom)
        {
            var locationStates = world
                .Locations
                .Select(x => new TrackerLocationState
                {
                    LocationId = x.Id,
                    Item = x.Item?.Type,
                    Cleared = false
                })
                .ToList();

            var regionStates = world
                .Regions
                .Select(x => new TrackerRegionState
                {
                    TypeName = x.GetType().Name,
                    Reward = x is IHasReward rewardRegion ? rewardRegion.Reward : null,
                    Medallion = x is INeedsMedallion medallionRegion ? medallionRegion.Medallion : null
                })
                .ToList();

            var state = new TrackerState()
            {
                LocationStates = locationStates,
                RegionStates = regionStates,
                StartDateTime = DateTimeOffset.Now,
                UpdatedDateTime = DateTimeOffset.Now
            };

            generatedRom.TrackerState = state;
            _randomizerContext.SaveChanges();

        }

        public TrackerState? LoadState(World world, GeneratedRom generatedRom)
        {
            var trackerState = generatedRom.TrackerState;

            if (trackerState == null)
            {
                return null;
            }

            _randomizerContext.Entry(trackerState).Collection(x => x.LocationStates).Load();

            foreach (var locationState in trackerState.LocationStates)
            {
                var location = world.Locations.First(x => x.Id == locationState.LocationId);
                location.State = locationState;
                location.Item = locationState.Item != null ? new Item(locationState.Item.Value, world) : null;
            }

            var locationCount = world.Locations.Count();
            var emptyStateLocations = world.Locations.Where(x => x.State == null).ToList();

            return trackerState;
        }


        public void SaveState(World world, GeneratedRom generatedRom, double secondsElapsed)
        {
            var trackerState = generatedRom.TrackerState;

            if (trackerState == null)
            {
                return;
            }

            var totalLocations = world.Locations.Count();
            var clearedLocations = world.Locations.Where(x => x.State.Cleared).Count();
            var percCleared = (int)Math.Floor((double)clearedLocations / totalLocations * 100);

            trackerState.UpdatedDateTime = DateTimeOffset.Now;
            trackerState.SecondsElapsed = secondsElapsed;
            trackerState.PercentageCleared = percCleared;

            _randomizerContext.SaveChanges();
        }
    }
}
