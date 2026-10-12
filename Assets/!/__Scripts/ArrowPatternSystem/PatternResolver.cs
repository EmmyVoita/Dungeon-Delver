using System.Collections.Generic;
using UnityEngine;

public class PatternResolver
{
    private readonly PatternDatabase _database;

    public PatternResolver(PatternDatabase database)
    {
        _database = database;
    }

    public List<ResolvedPattern> Resolve(IEnumerable<PatternBlock> blocks)
    {
        List<ResolvedPattern> resolved = new();

        foreach (PatternBlock block in blocks)
        {
            ArrowPattern pattern =_database.GetRandomPattern(block.PatternType,block.LengthInBeats);

            if (pattern == null)
                continue;

            resolved.Add(new ResolvedPattern(block.StartBeat,pattern)
            );
        }

        return resolved;
    }
}