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
    public static class TiberiumRimSettings
    {
        public static TiberiumSettings settings;
    }

    [StaticConstructorOnStartup]
    public class TiberiumRimMod : Mod
    {
        public TiberiumSettings settings;

        public Vector2 scrollPos = new Vector2();       

        public TiberiumRimMod(ModContentPack content) : base(content)
        {
            this.settings = GetSettings<TiberiumSettings>();
            TiberiumRimSettings.settings = this.settings;
        }

        public override string SettingsCategory() => "TiberiumRim";

        public override void DoSettingsWindowContents(Rect inRect)
        {
            //TODO: fix spacing for all languages
            float yOff = 25f;
            Widgets.DrawLine(new Vector2(inRect.width / 2, inRect.y -7f), new Vector2(inRect.width / 2, inRect.height + 75f), Color.gray, 1f);
            Rect winRect = new Rect(0f, yOff, inRect.width, inRect.height);
            Rect leftSide = new Rect(0f, winRect.y, winRect.width / 2, winRect.height);

            //GUI.BeginGroup(leftSide);
            MakeTitle(new Rect(5f, leftSide.y + 7f, leftSide.width, 17f), "SettingsGeneral".Translate());
            Rect rect1 = new Rect(0f, 20f, leftSide.width, 50).ContractedBy(5f);
            MakeNewCheckBox(rect1, "BuildingDamage".Translate(), ref this.settings.BuildingDamage, out rect1, "BuildingDamageDesc".Translate(), false, yOff);
            MakeNewCheckBox(rect1, "EntityDamage".Translate(), ref this.settings.EntityDamage, out rect1, "EntityDamageDesc".Translate(), false, yOff);
            MakeNewCheckBox(rect1, "PawnDamage".Translate(), ref this.settings.PawnDamage, out rect1, "PawnDamageDesc".Translate(), false, yOff);
            MakeNewCheckBox(rect1, "WorldSpread".Translate(), ref this.settings.WorldSpread, out rect1, "WorldSpreadDesc".Translate(), false, yOff);
            MakeNewCheckBox(rect1, "UseSpecificProducersLabel".Translate(), ref this.settings.UseSpecificProducers, out rect1, null, false, yOff);
            float height = 4 * yOff;
            Rect boxRect = new Rect(rect1.x, rect1.y + yOff + 10f, rect1.width, height + 10f);
            if (this.settings.UseSpecificProducers)
            {                
                Widgets.DrawShadowAround(boxRect);
                Widgets.DrawMenuSection(boxRect.ExpandedBy(1f));

                //Set all settings first
                foreach (TiberiumProducerDef def in DefDatabase<TiberiumProducerDef>.AllDefsListForReading)
                {
                    if (!this.settings.ProducerBools.Keys.Any(sw => sw.value == def.defName))
                    {
                        this.settings.ProducerBools.Add(def.defName, true);
                    }
                }              
                int setter = 0;                
                Rect rect2 = new Rect(boxRect.x, boxRect.y, boxRect.width / 2, rect1.height);
                float y2 = rect2.y;
                for (int i = 0; i < this.settings.ProducerBools.Count; i++)
                {
                    StringWrapper defName = this.settings.ProducerBools.Keys.ToList()[i];
                    setter++;
                    rect2.x = boxRect.x;
                    if (setter % 2 == 0)
                    { rect2.x = rect1.width / 2; }
                    else { rect2.y = y2 + (setter / 2) * yOff; }              
                    bool temp = this.settings.ProducerBools[defName];
                    MakeNewCheckBox(rect2, DefDatabase<TiberiumProducerDef>.GetNamed(defName).LabelCap, ref temp, out Rect rect);                  
                    this.settings.ProducerBools[defName] = temp;
                }
                rect1.y += boxRect.height + 10f;
            }           
            MakeNewCheckBox(rect1, "UseProducerCapLabel".Translate(), ref this.settings.UseProducerCap, out rect1, "UseProducerCapDesc".Translate(), false, yOff);
            if (this.settings.UseProducerCap)
            {
                Rect rect2 = rect1.ContractedBy(2f);
                rect2.y += yOff + 5;
                rect2.height -= 10f;
                string buffer = this.settings.TiberiumProducersAmt.ToString();
                Widgets.TextFieldNumeric(rect2, ref this.settings.TiberiumProducersAmt, ref buffer, 1f, 100f);
                rect1.y += rect2.height+5f ;
            }
            MakeNewCheckBox(rect1, "UseSpreadRadiusLabel".Translate(), ref this.settings.UseSpreadRadius, out rect1,"UseSpreadRadiusDesc".Translate(), false, yOff);
            MakeNewCheckBox(rect1, "UseCustomBackgroundLabel".Translate(), ref this.settings.UseCustomBackground, out rect1, "UseCustomBackgroundDesc".Translate(), false, yOff);
            //GUI.EndGroup();

            Rect rightSide = new Rect(winRect.width/2, winRect.y, winRect.width / 2, winRect.height);
            //GUI.BeginGroup(rightSide);
            MakeTitle(new Rect(rightSide.x + 5f, rightSide.y + 7f, rect1.width, 17f), "SettingsSpecial".Translate());
            Rect rect3 = new Rect(winRect.width / 2f, 65f, rightSide.width, 25).ContractedBy(5f);
            MakeSlider(rect3, "BuildingDmgMltp".Translate(), ref settings.BuildingDamageMltp, 0f, 10f, 15f, out rect3);
            MakeSlider(rect3, "ItemDmgMltp".Translate(), ref settings.ItemDamageMltp, 0f, 10f, 15f, out rect3);
            MakeSlider(rect3, "SpreadMltp".Translate(), ref settings.SpreadMltp, 0f, 5f, 15f, out rect3);
            MakeSlider(rect3, "TibGrowthRate".Translate(), ref settings.GrowthRate, 0.01f, 3f, 15f, out rect3);
            MakeSlider(rect3, "ExposureRate".Translate(), ref settings.InfectionTouch, 0.01f, 1f, 15f, out rect3);
            //MakeNewCheckBox(rect3, "ActiveEVA".Translate(), ref settings.activeEVA, out rect3, "ActiveEVADesc".Translate(), false, 15f);
            //EVASelector(new Rect(rect3.x, rect3.y, rect3.width, 20f), ref settings.EVASelector, 20f);


            //GUI.EndGroup();
            if (Widgets.ButtonText(new Rect(0f, inRect.height + 35f, 125f, 45f), "Reset Default"))
            {
                TiberiumRimSettings.settings.ResetToDefault();
            }

            if (Widgets.ButtonText(new Rect(130f, inRect.height + 35f, 125f, 45f), "Presets"))
            {
                Find.WindowStack.Add(new Dialog_Difficulty(delegate
                {
                    TiberiumRimSettings.settings.SetEasy();
                }, delegate
                {
                    TiberiumRimSettings.settings.ResetToDefault();
                },
                    delegate
                    {
                        TiberiumRimSettings.settings.SetHard();
                    }));
            }
        }

        public void MakeNewCheckBox(Rect rect, string label, ref bool checkOn, out Rect newRect, string desc = null, bool disabled = false, float yOffset = 0f, float xOffset = 0f)
        {
            Rect rect2 = rect;
            rect2.y += yOffset;
            rect2.x += xOffset;
            Widgets.CheckboxLabeled(rect2.ContractedBy(10f), label, ref checkOn, disabled);
            if (desc != null)
            {
                TooltipHandler.TipRegion(rect2.ContractedBy(10f), desc);
            }
            newRect = rect2;
        }

        public void MakeSlider(Rect rect, string label, ref float value, float min, float max, float yOffSet, out Rect newRect)
        {
            float roundTo = value < 1f ? 0.01f : 0.1f;
            int decimals = value < 1f ? 3 : 2;
            value = (float)Math.Round(Widgets.HorizontalSlider(rect, value, min, max, false, label + ": " + value, null, null, roundTo),decimals);
            rect.y += rect.height + yOffSet;
            newRect = rect;
        }

        //TODO: fix eva
        public static bool EVASelector(Rect rect, ref MultiCheckboxState state, float iconSize)
        {
            string faction;
            Texture2D tex;
            if (state == MultiCheckboxState.On)
            {
                faction = "GDI";
                tex = TRMats.GDI;
            }
            else if (state == MultiCheckboxState.Off)
            {
                faction = "Scrin";
                tex = TRMats.Scrin;
            }
            else
            {
                faction = "Nod";
                tex = TRMats.NOD;
            }
            Widgets.Label(rect, "Current EVA:".Translate());
            Rect rect2 = new Rect(100f, rect.y, rect.width, rect.height);
            Widgets.Label(rect2, faction);
            Rect iconRect = new Rect(rect2.x + 30f, rect2.y, iconSize, iconSize);
            MouseoverSounds.DoRegion(iconRect);
            if (Widgets.ButtonImage(iconRect, tex))
            {
                if(state == MultiCheckboxState.On)
                {
                    state = MultiCheckboxState.Partial;
                }
                if(state == MultiCheckboxState.Partial)
                {
                    state = MultiCheckboxState.Off;
                }
                if(state == MultiCheckboxState.Off)
                {
                    state = MultiCheckboxState.On;
                }
                return true;
            }
            return false;
        }

        public void MakeTitle(Rect rect, string label)
        {
            Text.Font = GameFont.Tiny;
            Widgets.Label(rect, label);
            Widgets.DrawLine(new Vector2(rect.xMin, rect.yMax), new Vector2(rect.xMin + rect.width, rect.yMax), Color.gray, 1f);
            Text.Font = GameFont.Small;
        }
    }

    public class TiberiumSettings : ModSettings
    {
        //General Settings:
        public bool BuildingDamage = true;
        public bool EntityDamage = true;
        public bool PawnDamage = true;
        public bool UseProducerCap = false;
        public bool UseSpecificProducers = false;

        public Dictionary<StringWrapper, bool> ProducerBools = new Dictionary<StringWrapper, bool>();

        public bool UseSpreadRadius = false;
        public bool UseCustomBackground = true;
        public int TiberiumProducersAmt = 7;

        public bool WorldSpread = true;

        //Specialized Settings
        public float InfectionTouch = 0.1f;
        public float BuildingDamageMltp = 1f;
        public float ItemDamageMltp = 1f;
        public float GrowthRate = 1f;
        public float SpreadMltp = 1f;

        public bool activeEVA = false;
        public MultiCheckboxState EVASelector = MultiCheckboxState.Off;

        public bool FirstStartUp = true;

        public void SetEasy()
        {
            BuildingDamage = false;
            EntityDamage = false;
            PawnDamage = true;
            UseProducerCap = true;
            UseSpecificProducers = false;
            UseSpreadRadius = true;
            UseCustomBackground = true;
            TiberiumProducersAmt = 5;
            WorldSpread = true;

            InfectionTouch = 0.01f;
            BuildingDamageMltp = 0.1f;
            ItemDamageMltp = 0.1f;
            GrowthRate = 0.5f;
            SpreadMltp = 0.25f;
        }

        public void ResetToDefault()
        {
            BuildingDamage = true;
            EntityDamage = true;
            PawnDamage = true;
            UseProducerCap = false;
            UseSpecificProducers = false;
            UseSpreadRadius = false;
            UseCustomBackground = true;
            TiberiumProducersAmt = 7;
            WorldSpread = true;

            InfectionTouch = 0.1f;
            BuildingDamageMltp = 1f;
            ItemDamageMltp = 1f;
            GrowthRate = 1f;
            SpreadMltp = 1f;
        }

        public void SetHard()
        {
            BuildingDamage = true;
            EntityDamage = true;
            PawnDamage = true;
            UseProducerCap = false;
            UseSpecificProducers = false;
            UseSpreadRadius = false;
            UseCustomBackground = true;
            TiberiumProducersAmt = 5;
            WorldSpread = true;

            InfectionTouch = 0.45f;
            BuildingDamageMltp = 4.5f;
            ItemDamageMltp = 4f;
            GrowthRate = 2f;
            SpreadMltp = 2.5f;
        }

        public void SetBool(ref bool b, bool set)
        {
            b = set;
        }

        public void SetValue(ref int i, int set)
        {
            i = set;
        }

        public override void ExposeData()
        {          
            base.ExposeData();
            Scribe_Values.Look(ref this.BuildingDamage, "BuildingDamage", true, true);
            Scribe_Values.Look(ref this.EntityDamage, "EntityDamage", true, true);
            Scribe_Values.Look(ref this.PawnDamage, "PawnDamage", true, true);
            Scribe_Values.Look(ref this.UseProducerCap, "UseProducerCap", false, true);
            Scribe_Values.Look(ref this.UseSpecificProducers, "UseSpecificProducers", false, true);
            Scribe_Collections.Look(ref this.ProducerBools, "ProducerBools", LookMode.Deep, LookMode.Value);
            Scribe_Values.Look(ref this.UseSpreadRadius, "UseSpreadRadius", false, true);
            Scribe_Values.Look(ref this.UseCustomBackground, "UseCustomBackground", true, true);
            Scribe_Values.Look(ref this.TiberiumProducersAmt, "TiberiumProducersAmt", 7, true);
            Scribe_Values.Look(ref this.FirstStartUp, "FirstStartUp", true, true);
            Scribe_Values.Look(ref this.WorldSpread, "WorldSpread", true, true);

            Scribe_Values.Look(ref this.activeEVA, "activeEVA", true, true);
            Scribe_Values.Look(ref this.EVASelector, "EVASelector");

            Scribe_Values.Look(ref this.SpreadMltp, "SpreadMltp", 1, true);
            Scribe_Values.Look(ref this.InfectionTouch, "InfectionTouch", 0.1f, true);
            Scribe_Values.Look(ref this.BuildingDamageMltp, "BuildingDamageMltp", 1f, true);
            Scribe_Values.Look(ref this.ItemDamageMltp, "ItemDamageMltp", 1f, true);
            Scribe_Values.Look(ref this.GrowthRate, "GrowthRate", 1f, true);
        }
    }
}