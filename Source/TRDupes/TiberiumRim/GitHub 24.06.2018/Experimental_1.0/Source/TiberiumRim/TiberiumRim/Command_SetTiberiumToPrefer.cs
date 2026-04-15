using System;
using System.Collections.Generic;
using System.Linq;
using RimWorld;
using Verse;
using UnityEngine;

namespace TiberiumRim
{
    [StaticConstructorOnStartup]
    public class Command_SetTiberiumToPrefer : Command
    {
        private static readonly Texture2D SetPlantToGrowTex = ContentFinder<Texture2D>.Get("UI/Commands/SetPlantToGrow", true);

        public IHarvestPreferenceSettable settable;

        private List<IHarvestPreferenceSettable> settables;

        public Map map;

        public override void ProcessInput(Event ev)
        {
            base.ProcessInput(ev);
            List<FloatMenuOption> list = new List<FloatMenuOption>();
            if(this.settables == null)
            {
                this.settables = new List<IHarvestPreferenceSettable>();
            }
            if (!this.settables.Contains(this.settable))
            {
                this.settables.Add(settable);
            }
            HashSet<TiberiumCrystalDef> typeSum = new HashSet<TiberiumCrystalDef>();
            foreach(TiberiumCrystalDef def in DefDatabase<TiberiumCrystalDef>.AllDefsListForReading)
            {
                if (TRUtils.AllTiberiumProducerTypesOnMap(map).Contains(def))
                {
                    typeSum.Add(def);
                }
                if (TRUtils.AllTiberiumCrystalTypesOnMap(map).Contains(def))
                {
                    if (!typeSum.Contains(def))
                    {
                        typeSum.Add(def);
                    }
                }
            }
            foreach (TiberiumCrystalDef tibDef in typeSum.Where((TiberiumCrystalDef x) => x.HarvestableType))
            {             
                string text = tibDef.LabelCap;
                list.Add(new FloatMenuOption(text, delegate
                {
                    for (int i = 0; i < this.settables.Count; i++)
                    {
                        this.settables[i].SetTiberiumDefToPrefer(tibDef);
                    }
                },
                MenuOptionPriority.Default, null, null, 29f, (Rect rect) => Widgets.InfoCardButton(rect.x + 5f, rect.y + (rect.height - 24f) / 2f, tibDef), null));
            }
            if (list.Count > 0)
            {
                Find.WindowStack.Add(new FloatMenu(list));
            }
        }

        public override bool InheritInteractionsFrom(Gizmo other)
        {
            if (this.settables == null)
            {
                this.settables = new List<IHarvestPreferenceSettable>();
            }
            this.settables.Add(((Command_SetTiberiumToPrefer)other).settable);
            return false;
        }
    }
}
