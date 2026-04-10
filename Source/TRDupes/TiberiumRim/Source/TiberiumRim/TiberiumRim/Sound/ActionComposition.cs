using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class ActionComposition
    {
        private List<ActionPart> actionParts = new List<ActionPart>();
        private Action FinishAction;
        public GlobalTargetInfo target;
        private int curTick = 0;
        private int startTick = 0;
        private int endTick;

        private readonly string compositionName;

        public ActionComposition(string name)
        {
            compositionName = name;
        }

        public void CacheMap(GlobalTargetInfo target)
        {
            this.target = target;
        }

        public void AddFinishAction(Action action)
        {
            FinishAction = action;
        }

        public void AddPart(Action<ActionPart> action, float time, float playTime = 0)
        {
            actionParts.Add(new ActionPart(this, action, time, playTime));
            Log.Message("[" + compositionName + "]Adding Action Part at " + time + " for " + playTime + "s");
        }

        public void AddPart(Action<ActionPart> action, SoundDef sound, SoundInfo info, float time, float playTime = 0)
        {
            actionParts.Add(new ActionPart(this, action, sound, info, time, playTime));
            Log.Message("[" + compositionName + "]Adding Action/Sound Part at " + time + " for " + playTime + "s");
        }

        public void AddPart(SoundDef sound, SoundInfo info, float time, float playTime = 0)
        {
            actionParts.Add(new ActionPart(this, sound, info, time, playTime));
            Log.Message("[" + compositionName + "]Adding Sound Part at " + time + " for " + playTime + "s");
        }

        public int CurrentTick => curTick;
        public int ActionCount => actionParts.Count;

        public void Init()
        {
            //TODO: Handle Exceptions (simultaneous actions etc) 
            if(actionParts.GroupBy(a => a.startTick).Any())
                Log.Error("Action Composition has simultaneous actions!");
            startTick = actionParts.First().startTick;
            endTick = actionParts.Last().endTick;

            Log.Message("Initializing ActionComposition starttick: " + startTick + " endTick: " + endTick);
            Current.Game.GetComponent<GameComponent_ActionCompManager>().InitComposition(this);
        }

        public void FinalizeComposition()
        {
            FinishAction?.Invoke();
            Current.Game.GetComponent<GameComponent_ActionCompManager>().RemoveComposition(this);
        }

        public void Tick()
        {
            if (actionParts.All(a => a.Completed))
            {
                FinalizeComposition();
                return;
            }
            for (var i = 0; i < actionParts.Count; i++)
            {
                var part = actionParts[i];
                part.Tick(curTick, i);
            }

            curTick++;
        }

        public override string ToString()
        {
            return "ActionComp '" + compositionName + "'";
        }
    }

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
            Log.Message("Part is Instant: " + Instant);
        }

        public ActionPart(ActionComposition parent, SoundDef sound, SoundInfo info, float time, float playTime = 0f)
        {
            parentComposition = parent;
            this.sound = new SoundPart(sound, info);
            this.startTick = time.SecondsToTicks();
            this.endTick = startTick + playTime.SecondsToTicks();
            this.playTime = playTime.SecondsToTicks();
            Log.Message("Part is Instant: " + Instant);
        }

        public ActionPart(ActionComposition parent, Action<ActionPart> action, SoundDef sound, SoundInfo info, float time, float playTime = 0f)
        {
            parentComposition = parent;
            this.action = action;
            this.sound = new SoundPart(sound, info);
            this.startTick = time.SecondsToTicks();
            this.endTick = startTick + playTime.SecondsToTicks();
            this.playTime = playTime.SecondsToTicks();
            Log.Message("Part is Instant: " + Instant);
        }

        public int CurrentTick => curTick;

        public bool Instant => startTick == endTick;

        public bool Completed { get => completed;  set => completed = value; }

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
                Log.Message("Should play sound now: " + sound?.def);
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
                Log.Message("Completing Part: " + (index+1) + "/" + parentComposition.ActionCount + " for " + parentComposition + " with " + actionCounter + " actions");
                Completed = true;
                return true;
            }
            return false;
        }
    }
}
