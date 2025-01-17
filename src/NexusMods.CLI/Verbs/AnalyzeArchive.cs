﻿using Microsoft.Extensions.Logging;
using NexusMods.CLI.DataOutputs;
using NexusMods.DataModel;
using NexusMods.DataModel.ArchiveContents;
using NexusMods.Paths;

namespace NexusMods.CLI.Verbs;

public class AnalyzeArchive : AVerb<AbsolutePath>
{
    private readonly IRenderer _renderer;
    private readonly FileContentsCache _archiveContentsCache;    
    public AnalyzeArchive(Configurator configurator, FileContentsCache archiveContentsCache, ILogger<AnalyzeArchive> logger)
    {
        _logger = logger;
        _renderer = configurator.Renderer;
        _archiveContentsCache = archiveContentsCache;
    }
    
    public static readonly VerbDefinition Definition = new("analyze-archive",
        "Analyzes the contents of an archive caches them, and outputs them", new[]
        {
            new OptionDefinition<AbsolutePath>("i", "inputFile", "File to Analyze")
        });

    private readonly ILogger<AnalyzeArchive> _logger;


    protected override async Task<int> Run(AbsolutePath inputFile, CancellationToken token)
    {
        try
        {
            var results = await _renderer.WithProgress(token, async () =>
            {
                var file = await _archiveContentsCache.AnalyzeFile(inputFile, token) as AnalyzedArchive;
                if (file == null) return Array.Empty<object[]>();
                return file.Contents.Select(kv =>
                {
                    return new object[]
                    {
                        kv.Key, kv.Value.Size, kv.Value.Hash,
                        string.Join(", ", kv.Value.FileTypes.Select(t => Enum.GetName(t)))
                    };
                });
            });

            await _renderer.Render(new Table(new[] { "Path", "Size", "Hash", "Signatures" },
                results.OrderBy(e => (RelativePath)e[0])));
        }
        catch (Exception ex)
        {
            _logger.LogCritical(ex, "While running");
            throw;
        }

        return 0;
    }
}