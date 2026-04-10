using System.Collections.Generic;
using RimWorld;
using TeleCore.GameData.Defs.Values;
using Verse;

namespace TeleCore.GameData.Defs.Extensions;

public class FactionDefExtension : DefModExtension
{
    public List<DefValueLoadable<FactionDef, int>> enemyTo;
}