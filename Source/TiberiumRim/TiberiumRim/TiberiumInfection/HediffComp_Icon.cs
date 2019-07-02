using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class HediffComp_Icon : HediffComp
    {
        private Texture2D icon;

        public HediffCompProperties_Icon Props => (HediffCompProperties_Icon)base.props;

        public override TextureAndColor CompStateIcon
        {
            get
            {
                if (icon == null)
                {
                    icon = ContentFinder<Texture2D>.Get(Props.iconPath);
                }
                return icon;
            }
        }
    }

    public class HediffCompProperties_Icon : HediffCompProperties
    {
        public HediffCompProperties_Icon()
        {
            compClass = typeof(HediffComp_Icon);
        }

        public string iconPath = "";
    }
}
