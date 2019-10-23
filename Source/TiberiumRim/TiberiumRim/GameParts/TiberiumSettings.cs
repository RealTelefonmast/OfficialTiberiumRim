using Harmony;
using System.Reflection;
using UnityEngine;
using System;
using System.Text;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using RimWorld.Planet;
using RimWorld.BaseGen;
using Verse;
using Verse.AI.Group;
using Verse.Sound;

namespace TiberiumRim
{
    public class TiberiumSettings : ModSettings
    {
        public bool CustomBackground = true;

        //Tiberium Settings
        public float InfectionMltp = 1f;
        public float BuildingDamageMltp = 1f;
        public float ItemDamageMltp = 1f;
        public float GrowthRate = 1f;
        public float SpreadMltp = 1f;

        //Graphics
        public GraphicsSettings graphicsSettings = new GraphicsSettings();

        //PlaySettings
        public bool ShowNetworkValues = false;
        public bool EVASystem = true;

        
        //Debug
        public bool ShowMapCompAlert = true;

        public bool startedOnce = false;

        public void SetValue<T>(ref T field, T value)
        {
            field = value;
        }

        public void SetEasy()
        {

        }

        public void SetMedium()
        {

        }

        public void SetHard()
        {

        }
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref graphicsSettings, "graphics");
            Scribe_Deep.Look(ref ShowNetworkValues, "ShowNetworkValues");
        }
    }
}
