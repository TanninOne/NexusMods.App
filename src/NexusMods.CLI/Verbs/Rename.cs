﻿using NexusMods.DataModel.Abstractions;
using NexusMods.DataModel.Loadouts;

namespace NexusMods.CLI.Verbs;

public class Rename : AVerb<Loadout, string>
{
    private readonly LoadoutManager _manager;

    public Rename(LoadoutManager manager)
    {
        _manager = manager;
    }
    public static VerbDefinition Definition = new("rename",
        "Rename a loadout id to a specific registry name", new OptionDefinition[]
        {
            new OptionDefinition<Loadout>("l", "loadout", "Loadout to assign a name"),
            new OptionDefinition<string>("n", "name", "Name to assign the loadout")
        });
    
    protected override async Task<int> Run(Loadout loadout, string name, CancellationToken token)
    {
        _manager.Alter(loadout.LoadoutId, _ => loadout, $"Renamed {loadout.DataStoreId} to {name}");
        return 0;
    }
}