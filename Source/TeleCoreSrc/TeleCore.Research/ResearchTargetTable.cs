// TODO: Decouple from TiberiumRim-specific types (TRUtils, TRLog, TRContent, TRCDefOf, TRThingDef)

using System.Collections.Generic;
using System.Linq;
using RimWorld;
using TeleCore.Defs;
using TeleCore.Types.Exposables;
using TeleCore.Types.Structs;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Research;

public class ResearchTargetTable : IExposable
{
    private readonly Dictionary<ThingDef, List<TResearchTaskDef>> tasksForThings = new();
    public Dictionary<TResearchTaskDef, ScribeList<Thing>> targets = new();

    public ResearchTargetTable()
    {
        foreach (var task in DefDatabase<TResearchTaskDef>.AllDefs)
        {
            targets.Add(task, new ScribeList<Thing>(new List<Thing>(), LookMode.Reference));
            if (task.PossibleMainTargets.NullOrEmpty()) continue;
            foreach (var thingDef in task.PossibleMainTargets)
                if (tasksForThings.ContainsKey(thingDef))
                    tasksForThings[thingDef].Add(task);
                else
                    tasksForThings.Add(thingDef, new List<TResearchTaskDef> { task });
        }

        GlobalEventHandler.ThingSpawned += RegisterNewTarget;
        GlobalEventHandler.ThingDespawned += DeregisterTarget;
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref targets, "researchTargets", LookMode.Def, LookMode.Deep);
        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            foreach (var task in DefDatabase<TResearchTaskDef>.AllDefs)
                if (!targets.ContainsKey(task))
                {
                    targets.Add(task, new ScribeList<Thing>(new List<Thing>(), LookMode.Reference));
                    foreach (var thingDef in task.PossibleMainTargets)
                        if (tasksForThings.ContainsKey(thingDef))
                            tasksForThings[thingDef].Add(task);
                        else
                            tasksForThings.Add(thingDef, new List<TResearchTaskDef> { task });
                }

            foreach (var target in targets)
                for (var index = target.Value.Count - 1; index >= 0; index--)
                {
                    var thing = target.Value[index];
                    if (thing == null)
                        target.Value.Remove(thing);
                }
        }
    }

    public IEnumerable<Thing> GetTargetsFor(TResearchTaskDef task)
    {
        return targets[task].Where(Available);
    }

    private void RegisterNewTarget(ThingStateChangedEventArgs args)
    {
        var thing = args.Thing;

        if (thing is Building b && (b.Faction?.IsPlayer ?? false))
            TRUtils.ResearchCreationTable().TryTrackConstructedOrClaimedBuilding(b.def);

        if (!tasksForThings.TryGetValue(thing.def, out var tasks)) return;
        foreach (var task in tasks)
        {
            if (!targets.ContainsKey(task))
            {
                TRLog.Error($"No target list for {task}");
                continue;
            }

            if (targets[task].Contains(thing)) return;
            targets[task].Add(thing);

            if (task.RelevantTargetStat != null)
                targets[task].SortBy(t => t?.GetStatValue(task.RelevantTargetStat) ?? 0);
        }
    }

    private void DeregisterTarget(ThingStateChangedEventArgs args)
    {
        var thing = args.Thing;
        if (!tasksForThings.ContainsKey(thing.def)) return;
        foreach (var task in tasksForThings[thing.def]) targets[task].Remove(thing);
    }

    private static bool Available(Thing thing)
    {
        var compThing = thing as ThingWithComps;
        if (compThing == null) return true;
        return compThing.IsPoweredOn(); //IsPowered(out bool usesPower) || !usesPower;
    }
}