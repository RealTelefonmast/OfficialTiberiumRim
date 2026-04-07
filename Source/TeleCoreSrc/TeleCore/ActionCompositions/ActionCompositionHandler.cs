using System.Collections.Generic;
using Verse;

namespace TeleCore.ActionCompositions;

public class ActionCompositionHandler : IExposable
{
    private static ActionCompositionHandler instance;

    //Local Instance
    private List<ActionComposition> currentCompositions = new();

    public ActionCompositionHandler()
    {
        instance = this;
    }

    public void ExposeData()
    {
        Scribe_Collections.Look(ref currentCompositions, nameof(currentCompositions), LookMode.Deep);
    }

    //Static Accessors
    public static void InitComposition(ActionComposition composition)
    {
        instance.currentCompositions.Add(composition);
    }

    public static void RemoveComposition(ActionComposition composition)
    {
        instance.currentCompositions.Remove(composition);
    }

    public void TickActionComps()
    {
        for (var i = currentCompositions.Count - 1; i >= 0; i--)
            currentCompositions[i].Tick();
    }
}