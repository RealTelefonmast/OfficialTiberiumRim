using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class SoundSample : Sample
    {
        public SoundInfo info;
        public SoundParams externalParams = new SoundParams();
        public SoundSample(SubSoundDef def) : base(def)
        {
        }

        public override float ParentStartRealTime
        {
            get
            {
                return this.startRealTime;
            }
        }

        public override float ParentStartTick
        {
            get
            {
                return (float)this.startTick;
            }
        }

        public override float ParentHashCode
        {
            get
            {
                return (float)this.GetHashCode();
            }
        }

        public override SoundParams ExternalParams
        {
            get
            {
                return this.externalParams;
            }
        }

        public override SoundInfo Info
        {
            get
            {
                return this.info;
            }
        }

        public static SoundSample TryMakeAndPlay(SubSoundDef def, AudioClip clip, SoundInfo info)
        {
            SoundSample sample = new SoundSample(def);
            sample.info = info;
            sample.source = Find.SoundRoot.sourcePool.GetSource(def.onCamera);
            if (sample.source == null)
                return null;
            sample.source.clip = clip;
            sample.source.volume = sample.SanitizedVolume;
            sample.source.pitch = sample.SanitizedPitch;
            sample.source.minDistance = sample.subDef.distRange.TrueMin;
            sample.source.maxDistance = sample.subDef.distRange.TrueMax;
            if (def.onCamera)
            {
                sample.source.spatialBlend = 0f;
            }
            else
            {
                sample.source.gameObject.transform.position = info.Maker.Cell.ToVector3ShiftedWithAltitude(0f);
                sample.source.minDistance = def.distRange.TrueMin;
                sample.source.maxDistance = def.distRange.TrueMax;
                sample.source.spatialBlend = 1f;
            }
            sample.Update();
            sample.source.Play();
            GameComponent_TR.TRComp().soundManager.TryAddSample(sample);
            return sample;
        }
    }
}
