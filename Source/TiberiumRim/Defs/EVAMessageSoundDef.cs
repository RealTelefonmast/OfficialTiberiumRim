using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TR.EVA;

public class EVAMessageSoundDef : SoundDef
{
    private readonly List<TypeFloat<SubSoundDef>> weightedSubSounds = new();

    public List<TypeFloat<string>> weightedSounds;

    public EVAType EVAType { get; }

    public EVASignal EVASignal { get; }

    public static EVAMessageSoundDef Named(string defName)
    {
        return DefDatabase<EVAMessageSoundDef>.GetNamed(defName, false);
    }


    public override void ResolveReferences()
    {
        //
        GameComponent_EVA.RegisterMessageDef(this);

        //
        LongEventHandler.ExecuteWhenFinished(delegate
        {
            foreach (var weightedSound in weightedSounds)
            {
                var soundGrain = new AudioGrain_Clip
                {
                    clipPath = weightedSound.type
                };

                var subSound = new SubSoundDef();
                subSound.parentDef = this;
                subSound.onCamera = true;
                subSound.volumeRange = new FloatRange(100f, 100f);
                subSound.sustainLoop = false;

                //
                subSound.grains = new List<AudioGrain> { soundGrain };
                subSound.resolvedGrains = soundGrain.GetResolvedGrains().ToList();
                subSound.distinctResolvedGrainsCount = subSound.resolvedGrains.Distinct().Count();
                subSound.numToAvoid = Mathf.FloorToInt(subSound.distinctResolvedGrainsCount / 2f);
                if (subSound.distinctResolvedGrainsCount >= 6)
                    subSound.numToAvoid++;

                subSounds.Add(subSound);
                weightedSubSounds.Add(new TypeFloat<SubSoundDef>(subSound, weightedSound.value));
            }
        });
    }

    public void PlayMessage(Map map)
    {
        var soundToPlay = weightedSubSounds.RandomElementByWeight(t => t.value).type;
        soundToPlay?.TryPlay(SoundInfo.OnCamera());
    }
}