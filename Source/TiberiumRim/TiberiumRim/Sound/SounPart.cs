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

        public SoundPart(SoundDef def, SoundInfo info, float second)
        {
            this.def = def;
            this.info = info;
        }

        public void PlaySound(int tick)
        {
            Log.Message("Playing Sound: " + def.defName + " at tick " + tick);
            def.PlayOneShot(info);
        }
    }
}
