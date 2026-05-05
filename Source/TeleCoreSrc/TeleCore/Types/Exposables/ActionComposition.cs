using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
using TeleCore.Types.Utils;
using Verse;
using Verse.Sound;

namespace TeleCore.Types.Exposables;

public class ActionComposition : IExposable, ILoadReferenceable
{
    internal static int _ID;

    private List<ActionPart> actionParts = new();

    private string compositionName;
    private int curTick, startTick, endTick;
    private Action finalAction;
    public GlobalTargetInfo target;

    public ActionComposition()
    {
        _ID += 1;
    }

    public ActionComposition(string name)
    {
        _ID += 1;
        compositionName = name;
    }

    public int CurrentTick => curTick;
    public int ActionCount => actionParts.Count;

    public void ExposeData()
    {
        Scribe_Values.Look(ref curTick, nameof(curTick));
        Scribe_Values.Look(ref startTick, nameof(startTick));
        Scribe_Values.Look(ref endTick, nameof(endTick));
        Scribe_Values.Look(ref compositionName, nameof(compositionName));
        Scribe_TargetInfo.Look(ref target, "target");
        //Scribe_Delegate.Look(ref finalAction, "finalAction");
        Scribe_Collections.Look(ref actionParts, "actionParts", LookMode.Deep);
    }

    public string GetUniqueLoadID()
    {
        return $"{nameof(ActionComposition)}{_ID}";
    }

    public void CacheMap(GlobalTargetInfo target)
    {
        this.target = target;
    }

    public void AddFinishAction(Action action)
    {
        finalAction = action;
    }

    public void AddPart(Action<ActionPart>? action, float? atSecond, float duration = 0)
    {
        actionParts.Add(new ActionPart(action, StartTime(atSecond), duration));
        //TLog.Debug("[" + compositionName + "]Adding Action Part at " + time + " for " + playTime + "s");
    }

    public void AddPart(Action<ActionPart>? action, SoundDef sound, SoundInfo info, float? atSecond, float duration = 0)
    {
        actionParts.Add(new ActionPart(action, sound, info, StartTime(atSecond), duration));
        //TLog.Debug("[" + compositionName + "]Adding Action/Sound Part at " + time + " for " + playTime + "s");
    }

    public void AddPart(SoundDef sound, SoundInfo info, float? atSecond, float duration = 0)
    {
        actionParts.Add(new ActionPart(null, sound, info, StartTime(atSecond), duration));
        //TLog.Debug("[" + compositionName + "]Adding Sound Part at " + time + " for " + playTime + "s");
    }

    private float StartTime(float? atSecond)
    {
        if (atSecond != null) return atSecond.Value;
        if (actionParts.Any()) return actionParts.Last().EndTick.TicksToSeconds();
        return 0;
    }

    public void Init()
    {
        startTick = actionParts.First().StartTick;
        endTick = actionParts.Last().EndTick;
        TLog.Message($"Init ActionComposition (\"{compositionName}\"): {startTick} -> {endTick}");
        ActionCompositionHandler.InitComposition(this);
    }

    public void FinalizeComposition()
    {
        finalAction?.Invoke();
        ActionCompositionHandler.RemoveComposition(this);
    }

    public void Tick()
    {
        if (actionParts.All(a => a.Completed))
        {
            FinalizeComposition();
            return;
        }

        if (curTick > endTick)
        {
            TLog.Warning($"Force completing \"{compositionName}\" at {curTick.TicksToSeconds()}s");
            FinalizeComposition();
            return;
        }

        for (var i = 0; i < actionParts.Count; i++)
            actionParts[i].Tick(curTick, i);
        curTick++;
    }

    public override string ToString()
    {
        return $"ActionComp '{compositionName}'";
    }
}