// TODO: Decouple from TiberiumRim-specific types (TRUtils, TRLog, TRContent, TRCDefOf, TRThingDef)

using System.Linq;
using RimWorld;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Unsorted;

public class BaseEvent : IExposable
{
    public EventDef def;
    private int endTick;
    private int startTick;

    public LookTargets EventTargets { get; set; }

    public string DescriptionWithTarget => ((string)def.description).Formatted(EventTargets.PrimaryTarget);

    protected virtual Map MapForEvent => Find.Maps.Where(m => m.IsPlayerHome).RandomElementByWeight(WeightForMap);

    public string[] DescArguments => null;

    public string TimeReadOut =>
        /*
            float days = ticksLeft.TicksToDays();
            float hours = GenDate.ToStringTicksToDays() GenDate.TicksPerHour;
            float minutes = ;
            float seconds = ;
            */
        (endTick - Find.TickManager.TicksGame).ToStringTicksToPeriodVerbose(true, false);


    public void ExposeData()
    {
        Scribe_Defs.Look(ref def, "props");
        Scribe_Values.Look(ref startTick, "startTick");
        Scribe_Values.Look(ref endTick, "endTick");
    }

    public void StartEvent(EventDef def)
    {
        this.def = def;
        startTick = Find.TickManager.TicksGame;
        endTick = startTick + def.ActiveTimeTicks;

        EventSetup();

        def.discoveries?.Discover();
        SendLetter(null, EventTargets);
    }

    public void SendLetter(IncidentParms parms, LookTargets targets)
    {
        def.letter?.SendLetter(parms, targets);
    }

    public void FinishEvent()
    {
        TRUtils.EventManager().Notify_EventFinished(this);
    }

    public void EventTick()
    {
        var tick = Find.TickManager.TicksGame;
        if (CanDoEventAction(tick)) EventAction();

        if (ShouldFinishNow(tick))
            FinishEvent();
    }

    public bool ShouldFinishNow(int curTick)
    {
        return startTick == endTick || curTick >= endTick;
    }

    public virtual void EventSetup()
    {
    }

    public virtual void EventAction()
    {
    }

    public virtual bool CanDoEventAction(int curTick)
    {
        return ShouldFinishNow(curTick);
    }

    protected virtual float WeightForMap(Map map)
    {
        return StorytellerUtility.DefaultThreatPointsNow(map);
    }
}