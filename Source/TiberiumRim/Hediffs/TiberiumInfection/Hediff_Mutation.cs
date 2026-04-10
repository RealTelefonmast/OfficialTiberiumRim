using TeleCore.Utils;
using TR.TextureContent;
using UnityEngine;
using Verse;

namespace TR.TiberiumInfection;

public class Hediff_Mutation : HediffWithComps
{
    private static readonly IntRange wanderRange = new(100, 750); //TODO: Change Back To Higher
    private Texture2D icon;

    private TiberiumMutation mutation;
    private int ticksLeft;

    public override TextureAndColor StateIcon
    {
        get
        {
            if (icon == null)
                icon = TiberiumContent.Hediff_Mutation;
            return icon;
        }
    }

    public override string LabelInBrackets => VisceralRisk().ToStringPercent();

    public override float PainOffset => 0.15f;

    private bool CanFinalize => 0 >= 1f;

    public override void PostMake()
    {
        base.PostMake();
        if (Part != null)
            Part = null;
        mutation = new TiberiumMutation();
    }

    public override void PostAdd(DamageInfo? dinfo)
    {
        base.PostAdd(dinfo);
        //TODO: Remove temporary mutation
        if (TRUtils.Chance(VisceralRisk()))
            pawn.health.AddHediff(HediffDef.Named("MutationDebuff"));
        else
            pawn.health.AddHediff(HediffDef.Named("MutationBuff"));
        pawn.health.RemoveHediff(this);
    }

    public override void Tick()
    {
        //TODO: Unlock Once part-based mutation is back
        return;
        base.Tick();
        //Spread mutation on the body, slowly mutate part by part
        if (ticksLeft <= 0)
        {
            Wander();
            ticksLeft = TRUtils.Range(wanderRange);
        }

        ticksLeft--;

        //Finalize mutation, occasionally turn animals into fiends and pawns into mutants
        if (CanFinalize)
            FinalizeMutation();
    }

    private void Wander()
    {
        //TODO: Reimplement part-based spread
        HediffDef hediffDef = null;
        var potentialPart = pawn.GetComp<Comp_TRHealthCheck>().partsForMutation.RandomElement();
        var risk = TRUtils.Chance(VisceralRisk());
    }

    private void FinalizeMutation()
    {
        //Chance based on Symbiotic Coverage
        //Turns animals into fiends, pawns into mutants
        if (TRUtils.Chance(Mathf.Pow(0, 2)))
            SymbioticAdaptation();
        //If unlucky, may lead to organ failure, cancer, or potentially forms a visceroid
        else

            return;
    }

    private void SymbioticAdaptation()
    {
        if (pawn.RaceProps.Animal)
        {
            var mutations = TRHediffDefOf.TiberiumFiendMutations;
            var kind = mutations.TiberiumFiendFor(pawn.kindDef);
            if (kind != null)
            {
                var request = new PawnGenerationRequest(kind);
                var newPawn = PawnGenerator.GeneratePawn(request);
                newPawn.ageTracker = pawn.ageTracker;
                newPawn.health = pawn.health;
                GenSpawn.Spawn(newPawn, pawn.Position, pawn.Map);
                pawn.DeSpawn();
                return;
            }
        }

        pawn.health.AddHediff(TRHediffDefOf.TiberiumImmunity);
    }

    //Pawn's health determines how well the mutation goes
    //Tib Mutation is naturally aggressive and bad though
    public float VisceralRisk()
    {
        //Naturally mutation is aggressive
        var num = 1f;
        //Pawn's health may add to bad mutation probability
        num += 1f - pawn.Health();
        //Crystallizing parts make up 1/3 of the mutation probability
        var hediffs = pawn.health.hediffSet.GetHediffs<Hediff_Crystallizing>().ToArray();
        if (hediffs.Any())
            num += hediffs.Sum(h => h.Severity) / hediffs.Count();
        //Being in Tiberium worsens the probability
        if (pawn.Position.GetTiberium(pawn.Map) != null)
            num += 0.75f;
        return num / 3f;
    }
}