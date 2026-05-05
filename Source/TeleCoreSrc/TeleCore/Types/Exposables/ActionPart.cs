using System;
using TeleCore.Sounds;
using TeleCore.Types.Utils;
using Verse;
using Verse.Sound;

namespace TeleCore.Types.Exposables;

public class ActionPart : IExposable
{
    private ScribeDelegate<Action<ActionPart>?> _ScribedDelegate;

    //
    private Action<ActionPart>? action;
    private bool completed;
    private int curTick;

    private SoundPart sound = SoundPart.Empty;

    //
    private int startTick, endTick, duration;

    public ActionPart()
    {
        //Sound
    }

    //Action Only
    public ActionPart(Action<ActionPart>? action, float startSeconds, float duration = 0f)
    {
        this.action = action;
        startTick = startSeconds.SecondsToTicks();
        endTick = startTick + duration.SecondsToTicks();
        this.duration = duration.SecondsToTicks();

        TLog.Message($"Making New ActionPart: {startTick} -> {endTick} | {this.duration}");
    }

    //Sound Only
    public ActionPart(SoundDef sound, SoundInfo info, float time, float duration = 0f)
    {
        this.sound = new SoundPart(sound, info);
        startTick = time.SecondsToTicks();
        endTick = startTick + duration.SecondsToTicks();
        this.duration = duration.SecondsToTicks();

        TLog.Message($"Making New ActionPart: {startTick} -> {endTick} | {this.duration}");
    }

    //Action & Sounds
    public ActionPart(Action<ActionPart>? action, SoundDef sound, SoundInfo info, float time, float duration = 0f)
    {
        this.action = action;
        this.sound = new SoundPart(sound, info);
        startTick = time.SecondsToTicks();
        endTick = startTick + duration.SecondsToTicks();
        this.duration = duration.SecondsToTicks();

        TLog.Message($"Making New ActionPart: {startTick} -> {endTick} | {this.duration}");
    }

    public int StartTick => startTick;
    public int EndTick => endTick;
    public int Duration => duration;

    public int CurrentTick => curTick;
    public bool Instant => startTick == endTick;

    public bool Completed
    {
        get => completed;
        private set => completed = value;
    }

    public void ExposeData()
    {
        //Ticks
        Scribe_Values.Look(ref curTick, "curTick");
        Scribe_Values.Look(ref startTick, "startTick");
        Scribe_Values.Look(ref endTick, "endTick");
        Scribe_Values.Look(ref duration, "duration");
        Scribe_Values.Look(ref completed, "completed");

        //Method
        if (Scribe.mode == LoadSaveMode.Saving) _ScribedDelegate = new ScribeDelegate<Action<ActionPart>?>(action);

        Scribe_Deep.Look(ref _ScribedDelegate, "partAction");

        if (Scribe.mode == LoadSaveMode.PostLoadInit) action = _ScribedDelegate.@delegate;
    }

    public bool CanBeDoneNow(int compositionTick)
    {
        return startTick <= compositionTick && compositionTick <= endTick;
    }

    public void Tick(int compositionTick, int partIndex = 0)
    {
        if (Completed || !CanBeDoneNow(compositionTick)) return;

        //Play Sound Once - Always
        if (CurrentTick == 0)
            sound.PlaySound();

        action?.Invoke(this);
        TryComplete(compositionTick, partIndex);

        curTick++;
    }

    private bool TryComplete(int compositionTick, int index = 0)
    {
        if (Instant || compositionTick == endTick)
        {
            Completed = true;
            TLog.Message($"Finishing actionpart[{startTick}]");
            return true;
        }

        return false;
    }
}