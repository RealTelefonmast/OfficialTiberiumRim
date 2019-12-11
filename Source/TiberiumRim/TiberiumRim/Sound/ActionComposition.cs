using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using RimWorld.Planet;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class ActionComposition
    {
        private List<ActionPart> actions = new List<ActionPart>();
        public GlobalTargetInfo target;
        public int curTick = 0;
        public int relativeTick = 0;
        private int lastTick = 0;
        public void AddPart(Action action, float time, float playTime = 0)
        {
            actions.Add(new ActionPart(action, time, playTime));
        }

        public void AddPart(Action action, SoundDef sound, SoundInfo info, float time, float playTime = 0)
        {
            actions.Add(new ActionPart(action, sound, info, time, playTime));
        }

        public void AddPart(SoundDef sound, SoundInfo info, float time, float playTime = 0)
        {
            actions.Add(new ActionPart(sound, info, time, playTime));
        }

        public void CacheMap(GlobalTargetInfo target)
        {
            this.target = target;
        }

        public void Init()
        {
            lastTick = Math.Max(actions.Max(a => a.playTick), actions.Max(a => a.endTick));
            Current.Game.GetComponent<GameComponent_ActionCompManager>().InitComposition(this);
        }

        public void FinalizeComposition()
        {
            Current.Game.GetComponent<GameComponent_ActionCompManager>().RemoveComposition(this);
        }

        public void Tick()
        {
            if (curTick == lastTick)
                FinalizeComposition();
            Log.Message("Main Tick: " + curTick + " Relative Tick: " + relativeTick);

            foreach (var action in actions)
            {
                if (action.Completed) continue;
                if (action.TryDoAction(curTick, actions.IndexOf(action), out int tick))
                    relativeTick = tick;
            }
            curTick++;
        }
    }

    public class ActionPart
    {
        public Action action;
        public SoundPart sound;
        public int playTick;
        public int endTick;

        public ActionPart(Action action, float time, float playTime = 0)
        {
            this.action = action;
            this.playTick = time.SecondsToTicks();
            endTick = playTick + playTime.SecondsToTicks();
            Log.Message("Making action Part: " + action + " playTick: " + playTick + " endTick: " + endTick);
        }

        public ActionPart(Action action, SoundDef def, SoundInfo info, float time, float playTime = 0)
        {
            this.action = action;
            this.sound = new SoundPart(def, info, time);
            this.playTick = time.SecondsToTicks();
            endTick = playTick + playTime.SecondsToTicks();
            Log.Message("Making actionsound Part: " + action + " sound: " + def.defName + " playTick: " + playTick + " endTick: " + endTick);
        }

        public ActionPart(SoundDef def, SoundInfo info, float time, float playTime = 0)
        {
            this.sound = new SoundPart(def, info, time);
            this.playTick = time.SecondsToTicks();
            endTick = playTick + playTime.SecondsToTicks();
            Log.Message("Making sound Part: " + " sound: " + def.defName + " playTick: " + playTick + " endTick: " + endTick);
        }

        public bool TryDoAction(int curTick, int num, out int tick)
        {
            tick = curTick - playTick;
            if(curTick == playTick)
                sound?.PlaySound(curTick);
            if (!CanDoNow(curTick)) return false;
            action?.Invoke();
            TryComplete(curTick, num);
            return true;
        }

        public bool CanDoNow(int tick)
        {
            return tick >= playTick && tick <= endTick;
        }

        public void TryComplete(int tick, int num)
        {
            Completed = endTick == playTick ? tick == playTick : tick == endTick;
            if(Completed)
                Log.Message("Completing Part: " + num);
        }

        public bool Completed { get; private set; } = false;
    }
}
