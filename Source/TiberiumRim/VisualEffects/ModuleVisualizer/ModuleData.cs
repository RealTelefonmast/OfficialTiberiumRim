using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HarmonyLib;
using UnityEngine;
using Verse;
using Verse.Noise;

namespace TiberiumRim
{
    public class ModuleData
    {
        private ModuleBase module;
        private Dictionary<string, Traverse> fields = new Dictionary<string, Traverse>();

        public ModuleData(ModuleBase module)
        {
            this.module = module;
            var moduleRefl = Traverse.Create(module);
            var fieldNames = moduleRefl.Fields();
            if (fieldNames.NullOrEmpty()) return;
            foreach (var field in fieldNames)
            {
                fields.Add(field, moduleRefl.Field(field));
            }
        }

        public void DrawData(Rect rect)
        {
            GUI.BeginGroup(rect);
            //Draw Data
            Rect titleRect = new Rect(0, 0, rect.width, 25);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, module.GetType().ToString().Split('.').Last());
            Text.Font = GameFont.Small;

            //
            if (fields.NullOrEmpty())
            {
                GUI.EndGroup();
                return;
            }
            float curY = 25;
            foreach (var field in fields)
            {
                Rect labelRect = new Rect(2.5f, curY, rect.width, rect.height);
                Widgets.Label(labelRect, field.Key);
                curY += 16;
            }
            GUI.EndGroup();
        }
    }
}
