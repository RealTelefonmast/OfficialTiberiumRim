using UnityEngine;
using Verse;

namespace TR;

// Crystal head overlay for humanlike pawns — uses head mesh set.
// Texture path: Pawns/TiberiumMutant/Heads/Mutant_head
public class PawnRenderNode_TibHead : PawnRenderNode
{
    public PawnRenderNode_TibHead(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props,
        tree)
    {
    }

    public override GraphicMeshSet MeshSetFor(Pawn pawn)
    {
        return HumanlikeMeshPoolUtility.GetHumanlikeHeadSetForPawn(pawn);
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        if (!pawn.health.hediffSet.HasHead)
            return null;

        return GraphicDatabase.Get<Graphic_Multi>("Pawns/TiberiumMutant/Heads/Mutant_head", ShaderFor(pawn),
            Vector2.one, ColorFor(pawn));
    }
}

// Crystal body overlay — humanlike uses bodyType-derived path, non-humanlike appends "_TibBody" to the animal's naked graphic path.
// Humanlike texture path: Pawns/TiberiumMutant/Bodies/<BodyTypeDef.defName>
// Non-humanlike texture path: <nakedGraphic.path>_TibBody
public class PawnRenderNode_TibBody : PawnRenderNode
{
    public PawnRenderNode_TibBody(Pawn pawn, PawnRenderNodeProperties props, PawnRenderTree tree) : base(pawn, props,
        tree)
    {
    }

    public override Graphic GraphicFor(Pawn pawn)
    {
        if (pawn.RaceProps.Humanlike)
            return GraphicForHumanlike(pawn);

        return GraphicForAnimal(pawn);
    }

    private Graphic GraphicForHumanlike(Pawn pawn)
    {
        var bodyDefName = pawn.story?.bodyType?.defName;
        if (bodyDefName == null)
            return null;

        return GraphicDatabase.Get<Graphic_Multi>($"Pawns/TiberiumMutant/Bodies/{bodyDefName}", ShaderFor(pawn),
            Vector2.one, ColorFor(pawn));
    }

    private Graphic GraphicForAnimal(Pawn pawn)
    {
        var nakedGraphic = pawn.Drawer.renderer.BodyGraphic;
        if (nakedGraphic == null)
            return null;

        var texPath = $"{nakedGraphic.path}_TibBody";
        if (ContentFinder<Texture2D>.Get($"{texPath}_north", false) == null)
            return null;

        return GraphicDatabase.Get<Graphic_Multi>(
            texPath, ShaderFor(pawn), nakedGraphic.drawSize, ColorFor(pawn));
    }
}