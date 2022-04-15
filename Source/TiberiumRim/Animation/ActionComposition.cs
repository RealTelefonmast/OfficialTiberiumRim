using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld.Planet;
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
            TLog.Debug("[" + compositionName + "]Adding Action Part at " + time + " for " + playTime + "s");
        }

        public void AddPart(Action<ActionPart> action, SoundDef sound, SoundInfo info, float time, float playTime = 0)
        {
            actionParts.Add(new ActionPart(this, action, sound, info, time, playTime));
            TLog.Debug("[" + compositionName + "]Adding Action/Sound Part at " + time + " for " + playTime + "s");
        }

        public void AddPart(SoundDef sound, SoundInfo info, float time, float playTime = 0)
        {
            actionParts.Add(new ActionPart(this, sound, info, time, playTime));
            TLog.Debug("[" + compositionName + "]Adding Sound Part at " + time + " for " + playTime + "s");
        }

        public int CurrentTick => curTick;
        public int ActionCount => actionParts.Count;

        public void Init()
        {
            //TODO: Handle Exceptions (simultaneous actions etc) 
            for (var i = 0; i < actionParts.Count; i++)
            {
                var action = actionParts[i];
                for (var k = 0; k < actionParts.Count; k++)
                {
                    var action2 = actionParts[k];
                    if (action.startTick == action2.startTick)
                    {
                        TLog.Error("Action " + i + " is simultanous to action " + k + "!");
                    }
                }
            }
            startTick = actionParts.First().startTick;
            endTick = actionParts.Last().endTick;

            TLog.Debug("Initializing ActionComposition starttick: " + startTick + " endTick: " + endTick);
            GameComponent_TR.TRComp().ActionCompositionHolder.InitComposition(this);
        }

        public void FinalizeComposition()
        {
            FinishAction?.Invoke();
            GameComponent_TR.TRComp().ActionCompositionHolder.RemoveComposition(this);
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
}
