using System.Collections.Generic;
using Verse;
using Verse.Sound;

namespace TiberiumRim;

public class SampleManager
{
    public List<SoundSample> cleanupList = new();
    public List<SoundSample> samples = new();

    public void Update()
    {
        cleanupList.Clear();
        for (var i = samples.Count - 1; i >= 0; i--)
        {
            var sample = samples[i];
            sample.Update();
            if (sample.source == null || !sample.source.isPlaying ||
                !SoundDefHelper.CorrectContextNow(sample.subDef.parentDef, sample.Map))
            {
                if (sample.source != null && sample.source.isPlaying) sample.source.Stop();

                sample.SampleCleanup();
                samples.Remove(sample);
            }
        }
    }

    public void TryAddSample(SoundSample sample)
    {
        var count = samples.Count(t => t.subDef == sample.subDef);
        if (count < sample.subDef.parentDef.maxVoices) samples.Add(sample);
    }

    public bool CanAddPlayingOneShot(SoundDef def, SoundInfo info)
    {
        return true;
    }
}