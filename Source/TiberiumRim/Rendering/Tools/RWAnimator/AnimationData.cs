using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public struct AnimationSet
    {
        private string tag;
        //private List<KeyFrame> ;

    }

    public class AnimationData : IExposable
    {
        public List<TextureData> allParts;

       // public List<>

       public void ExposeData()
       {
           Scribe_Collections.Look(ref allParts, "allParts", LookMode.Deep);
       }
    }
}
