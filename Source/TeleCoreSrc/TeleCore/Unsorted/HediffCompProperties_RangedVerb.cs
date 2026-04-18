using System.Collections.Generic;
using System.Linq;
using TeleCore.Hediffs;
using Verse;

namespace TeleCore.Unsorted;

public class HediffCompProperties_RangedVerb : HediffCompProperties
{
    public List<VerbProperties_Extended> verbs;

    public HediffCompProperties_RangedVerb()
    {
        compClass = typeof(HediffComp_RangedVerb);
    }

    public IEnumerable<VerbProperties> VerbsBase => verbs.Select(v => v as VerbProperties);
}