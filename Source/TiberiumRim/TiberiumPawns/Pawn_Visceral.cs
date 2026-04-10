using System.Text;
using Verse;

namespace TR;

public class Pawn_Visceral : TiberiumPawn
{
    private string pawnKindName = "";
    private string pawnName = "";

    public void Remember(string kindName, string name = "")
    {
        pawnName = name;
        pawnKindName = kindName;
    }

    public Visceroid BecomeVisceroid()
    {
        var generationRequest = new PawnGenerationRequest(PawnKindDef.Named("Visceroid"), Faction);
        var visceral = (Visceroid)PawnGenerator.GeneratePawn(generationRequest);
        visceral.ageTracker = ageTracker;
        visceral.Remember(pawnKindName, pawnName);
        return visceral;
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
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