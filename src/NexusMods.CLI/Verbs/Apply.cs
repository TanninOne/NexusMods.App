﻿using NexusMods.CLI.DataOutputs;
using NexusMods.DataModel.Extensions;
using NexusMods.DataModel.ModLists.ApplySteps;
using NexusMods.DataModel.ModLists.Markers;
using NexusMods.DataModel.ModLists.ModFiles;
using NexusMods.Paths;

namespace NexusMods.CLI.Verbs;

public class Apply
{
    private readonly IRenderer _renderer;
    public Apply(Configurator configurator)
    {
        _renderer = configurator.Renderer;
    }

    public static VerbDefinition Definition => new VerbDefinition("apply", "Apply a modlist to a game folder", new OptionDefinition[]
    {
        new OptionDefinition<ModListMarker>("m", "modList", "Mod List to apply"),
        new OptionDefinition<bool>("r", "run", "Run the application? (defaults to just printing the steps)"),
        new OptionDefinition<bool>("s", "summary", "Print the summary, not the detailed step list")
    });
    
    public async Task Run(ModListMarker modList, bool run, bool summary, CancellationToken token)
    {

        var steps = await modList.MakeApplyPlan(token).ToList();

        if (summary)
        {
            var rows = steps.GroupBy(s => s.GetType())
                .Select(g =>
                    new object[]
                    {
                        g.Key.Name, g.Count(), g.OfType<IStaticFileStep>().Aggregate((Size)0L, (o, n) => o + n.Size)
                    });
            await _renderer.Render(new Table(new[] { "Action", "Count", "Size"}, rows));
        }
        else
        {
            var rows = new List<object[]>();
            foreach (var step in steps)
            {
                if (step is IStaticFileStep smf)
                {
                    rows.Add(new object[]{step, step.To, smf.Hash, smf.Size});
                }
                else
                {
                    rows.Add(new object[]{step, step.To, null, default});
                }
            
            }
            await _renderer.Render(new Table(new[] { "Action", "To", "Hash", "Size"}, rows));
        }

        if (run) {
            await _renderer.WithProgress(token, async () =>
            {
                await modList.ApplyPlan(steps, token);
                return steps;
            });
        }

    }
}