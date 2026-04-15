using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class Visceroid : Pawn
    {
        public string kindDefName = "";

        public string oldName = "";

        public bool wasNamed = false;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Values.Look<string>(ref kindDefName, "kindDefName");
            Scribe_Values.Look<string>(ref oldName, "oldName");
            Scribe_Values.Look<bool>(ref wasNamed, "wasNamed");
        }

        public override string GetInspectString()
        {
            StringBuilder stringBuilder = new StringBuilder();
            stringBuilder.AppendLine(base.GetInspectString());
            if (wasNamed)
            {
                stringBuilder.AppendLine("UsedToBeName".Translate(new object[] { oldName, kindDefName }) + ".");
            }
            else
            if (kindDefName.Count() > 0)
            {
                stringBuilder.AppendLine("UsedToBeNoName".Translate(new object[] { kindDefName }) + ".");
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }

        public bool OtherVisceroidExists(out Visceroid visceroid)
        {
            Visceroid visc = (Visceroid)Map.listerThings.AllThings.Find((Thing x) => (x as Visceroid) != null && x != this && this.CanReserve(x));
            if(visc != null)
            {
                visceroid = visc;
                return true;
            }
            visceroid = null;
            return false;
        }
    }
}
