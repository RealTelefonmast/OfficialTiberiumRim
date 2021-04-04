using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using HarmonyLib;
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
            if(Find.SoundRoot.oneShotManager.CanAddPlayingOneShot(def, info))
                def.PlayOneShot(info);
        }
    }
}
