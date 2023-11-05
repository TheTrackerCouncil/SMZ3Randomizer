using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Randomizer.Abstractions;
using Randomizer.Shared;

namespace Randomizer.SMZ3.FileData.Patches
{
    public sealed class RomPatchFactory
    {
        private readonly IServiceProvider _services;

        public RomPatchFactory(IServiceProvider services)
        {
            _services = services;
        }

        public IEnumerable<RomPatch> GetPatches()
        {
            return _services.GetServices<RomPatch>()
                .Where(x => x.GetType().GetCustomAttribute<ManualAttribute>() == null)
                .Select(x => (patch: x, order: x.GetType().GetCustomAttribute<OrderAttribute>()?.Order ?? 0))
                .OrderBy(x => x.order)
                .Select(x => x.patch);
        }
    }
}
