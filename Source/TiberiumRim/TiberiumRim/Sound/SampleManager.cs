using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class SampleManager
    {
        public List<SoundSample> samples = new List<SoundSample>();
        public List<SoundSample> cleanupList = new List<SoundSample>();

        public void Update()
        {
            cleanupList.Clear();
            for (int i = this.samples.Count - 1; i >= 0; i--)
            {
                var sample = samples[i];
                sample.Update();
                if (sample.source == null || !sample.source.isPlaying ||
                    !SoundDefHelper.CorrectContextNow(sample.subDef.parentDef, sample.Map))
                {
                    if (sample.source != null && sample.source.isPlaying)
                    {
                        sample.source.Stop();
                    }

                    sample.SampleCleanup();
                    samples.Remove(sample);
                }
            }
        }

        public void TryAddSample(SoundSample sample)
        {
            int count = samples.Count(t => t.subDef == sample.subDef);
            if (count < sample.subDef.parentDef.maxVoices)
            {
                this.samples.Add(sample);
            }
        }

        public bool CanAddPlayingOneShot(SoundDef def, SoundInfo info)
        {
            return true;
        }
    }
}
