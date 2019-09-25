using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class TRSubSoundDef : SubSoundDef
    {
        public override void TryPlay(SoundInfo info)
        {

            //TODO: Check Can Add
            ResolvedGrain resolvedGrain = this.RandomizedResolvedGrain();
            ResolvedGrain_Clip resolvedGrain_Clip = resolvedGrain as ResolvedGrain_Clip;
            if (resolvedGrain_Clip != null)
            {
                if (SoundSample.TryMakeAndPlay(this, resolvedGrain_Clip.clip, info) == null)
                {
                    return;
                }
                SoundSlotManager.Notify_Played(this.parentDef.slot, resolvedGrain_Clip.clip.length);
            }
        }
    }
}
