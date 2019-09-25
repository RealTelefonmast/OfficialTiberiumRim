using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class TRSoundDef : SoundDef
    {
        public List<TRSubSoundDef> sounds;

        public void Play(SoundInfo info)
        {
            for (int i = 0; i < sounds.Count; i++)
            {
                sounds[i].TryPlay(info);
            }
        }
    }
}
