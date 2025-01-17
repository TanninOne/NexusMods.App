﻿using NexusMods.Hashing.xxHash64;
using NexusMods.Paths;

namespace NexusMods.DataModel.Loadouts.ApplySteps;

public class DeleteFile : IApplyStep, IStaticFileStep
{
    public required AbsolutePath To { get; init; }
    public required Hash Hash { get; init; }
    public required Size Size { get; init; }
}