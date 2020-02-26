using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class SoundPart
    {
        public SoundDef def;
        public SoundInfo info;

        public SoundPart(SoundDef def, SoundInfo info)
        {
            this.def = def;
            this.info = info;
        }

        public void PlaySound(int tick)
        {
            Log.Message("Playing Sound: " + def.defName + " at tick " + tick + " with info " + info.Maker);
            Log.Message("Current playing oneshots: " + Find.SoundRoot.oneShotManager.PlayingOneShots.ToStringSafeEnumerable());
            if(Find.SoundRoot.oneShotManager.CanAddPlayingOneShot(def, info))
                def.PlayOneShot(info);
        }
    }
}
