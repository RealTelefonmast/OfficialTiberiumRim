using RimWorld;
using UnityEngine;
using Verse;

namespace TR
{
    public class TRMainButtonDef : MainButtonDef
    {
        [Unsaved(false)] private Texture2D specialIcon;
        public string specialIconPath;

        public Texture2D SpecialIcon
        {
            get
            {
                if (specialIconPath == null) return null;
                return specialIcon ??= ContentFinder<Texture2D>.Get(specialIconPath, false);
            }
        }
    }
}
