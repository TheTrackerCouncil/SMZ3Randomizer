using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using TrackerCouncil.Shared;
using TrackerCouncil.Smz3.Shared;

namespace TrackerCouncil.Smz3.SeedGenerator.FileData.Patches;

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
            .Select(x => (patch: x, order: x.GetType().GetCustomAttribute<OrderAttribute>()?.Order ?? 0))
            .OrderBy(x => x.order)
            .Select(x => x.patch);
    }
}
