using System.Collections.Generic;
using RimWorld;
using TeleCore.Types.Exposables;
using Verse;

namespace TeleCore.Defs;

public class FactionDefExtension : DefModExtension
{
    public List<DefValueLoadable<FactionDef, int>> enemyTo;
}