using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using RimWorld;
using UnityEngine;
using Verse.Sound;

namespace TiberiumRim
{
    public class ActionCompositionHolder
    {
        private List<ActionComposition> Compositions = new List<ActionComposition>();

        public void InitComposition(ActionComposition composition)
        {
            Compositions.Add(composition);
        }

        public void RemoveComposition(ActionComposition composition)
        {
            Compositions.Remove(composition);
        }

        public void TickActionComps()
        {
            for (int i = Compositions.Count - 1; i >= 0; i--)
            {
                Compositions[i].Tick();
            }
        }
    }
}

