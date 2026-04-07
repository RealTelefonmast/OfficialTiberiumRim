using System.Collections.Generic;
using RimWorld;
using TeleCore.Defs.Values;
using Verse;

namespace TeleCore.Defs.Extensions;

public class FactionDefExtension : DefModExtension
{
    public List<DefValueLoadable<FactionDef, int>> enemyTo;
}