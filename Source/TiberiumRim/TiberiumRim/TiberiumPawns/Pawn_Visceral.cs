using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;

namespace TiberiumRim
{
    public class Pawn_Visceral : TiberiumPawn
    {
        private string pawnName = "";
        private string pawnKindName = "";

        public void Remember(string kindName, string name = "")
        {
            pawnName = name;
            pawnKindName = kindName;
        }

        public Visceroid BecomeVisceroid()
        {
            PawnGenerationRequest generationRequest = new PawnGenerationRequest(PawnKindDef.Named("Visceroid"), Faction);
            Visceroid visceral = (Visceroid)PawnGenerator.GeneratePawn(generationRequest);
            visceral.ageTracker = this.ageTracker;
            visceral.Remember(pawnKindName, pawnName);
            return visceral;
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            if (!pawnName.NullOrEmpty())
                sb.AppendLine("TR_VisceralMemoryName".Translate(pawnName));
            if (!pawnName.NullOrEmpty())
                sb.AppendLine("TR_VisceralMemoryKind".Translate(pawnKindName));
            if (!pawnName.NullOrEmpty() && !pawnName.NullOrEmpty())
                sb.AppendLine("TR_VisceralMemoryKindName".Translate(pawnName, pawnKindName));
            return sb.ToString().TrimEndNewlines();
        }
    }
}
