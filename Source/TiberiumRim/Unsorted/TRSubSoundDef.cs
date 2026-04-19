using Verse.Sound;

namespace TiberiumRim;

public class TRSubSoundDef : SubSoundDef
{
    public override void TryPlay(SoundInfo info)
    {
        //TODO: Check Can Add
        var resolvedGrain = RandomizedResolvedGrain();
        var resolvedGrain_Clip = resolvedGrain as ResolvedGrain_Clip;
        if (resolvedGrain_Clip != null)
        {
            if (SoundSample.TryMakeAndPlay(this, resolvedGrain_Clip.clip, info) == null) return;
            SoundSlotManager.Notify_Played(parentDef.slot, resolvedGrain_Clip.clip.length);
        }
    }
}