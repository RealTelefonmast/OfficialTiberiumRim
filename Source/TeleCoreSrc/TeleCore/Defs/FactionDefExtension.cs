using System.Collections.Generic;
using RimWorld;
using TeleCore.Unsorted;
using Verse;

namespace TeleCore.Defs;

public class FactionDefExtension : DefModExtension
{
    public List<DefValueLoadable<FactionDef, int>> enemyTo;
}