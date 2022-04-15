using System;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class ActionPart
    {
        public int startTick = 0;
        public int endTick = 0;
        public int playTime = 0;

        private int curTick = 0;
        public Action<ActionPart> action;
        public SoundPart sound;

        private readonly ActionComposition parentComposition;
        private bool completed = false;

        public ActionPart(ActionComposition parent)
        {
            this.parentComposition = parent;
        }

        public ActionPart(ActionComposition parent, Action<ActionPart> action, float time, float playTime = 0f)
        {
            parentComposition = parent;
            this.action = action;
            this.startTick = time.SecondsToTicks();
            this.endTick = startTick + playTime.SecondsToTicks();
            this.playTime = playTime.SecondsToTicks();
            TLog.Debug("Part is Instant: " + Instant);
        }

        public ActionPart(ActionComposition parent, SoundDef sound, SoundInfo info, float time, float playTime = 0f)
        {
            parentComposition = parent;
            this.sound = new SoundPart(sound, info);
            this.startTick = time.SecondsToTicks();
            this.endTick = startTick + playTime.SecondsToTicks();
            this.playTime = playTime.SecondsToTicks();
            TLog.Debug("Part is Instant: " + Instant);
        }

        public ActionPart(ActionComposition parent, Action<ActionPart> action, SoundDef sound, SoundInfo info, float time, float playTime = 0f)
        {
            parentComposition = parent;
            this.action = action;
            this.sound = new SoundPart(sound, info);
            this.startTick = time.SecondsToTicks();
            this.endTick = startTick + playTime.SecondsToTicks();
            this.playTime = playTime.SecondsToTicks();
            TLog.Debug("Part is Instant: " + Instant);
        }

        public int CurrentTick => curTick;

        public bool Instant => startTick == endTick;

        public bool Completed { get => completed; set => completed = value; }

        public bool CanBeDoneNow(int compositionTick)
        {
            return startTick <= compositionTick && compositionTick <= endTick;
        }

        private int actionCounter = 0;

        public void Tick(int compositionTick, int partIndex = 0)
        {
            //Log.Message("[Part " + (partIndex + 1) + "] Ticking At: " + compositionTick + " with relative tick: " + relativeTick);
            if (Completed || !CanBeDoneNow(compositionTick)) return;
            //Play Sound Once - Always
            if (CurrentTick == 0)
            {
                TLog.Debug("Should play sound now: " + sound?.def);
                sound?.PlaySound(compositionTick);
            }

            action?.Invoke(this);
            actionCounter++;

            TryComplete(compositionTick, partIndex);
            curTick++;
        }

        private bool TryComplete(int compositionTick, int index = 0)
        {
            if (Instant || compositionTick == endTick)
            {
                TLog.Debug("Completing Part: " + (index + 1) + "/" + parentComposition.ActionCount + " for " + parentComposition + " with " + actionCounter + " actions");
                Completed = true;
                return true;
            }
            return false;
        }
    }
}
