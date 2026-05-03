using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public enum VisceralStage
{
    Empty,
    Fresh,
    Corpse,
    Horror,
    Visceroid
}

public class VisceralPod : TiberiumThing, IThingHolder
{
    private bool hatched;
    private ThingOwner InnerContainer;
    private string kindName;
    private string pawnName;
    private float pawnSize;
    private bool prematureHatch;
    private int ticksLeft = -1;

    public Thing HeldThing => InnerContainer[0];

    private Pawn InnerPawn
    {
        get
        {
            if (HeldThing is Corpse) return (HeldThing as Corpse).InnerPawn;
            return HeldThing as Pawn;
        }
    }

    public VisceralStage VisceralStage
    {
        get
        {
            if (HeldThing != null)
            {
                //Fresh Pawn
                if (HeldThing is Pawn p && !(p is TiberiumPawn)) return VisceralStage.Fresh;
                //Not so fresh Pawn
                if (HeldThing is Corpse && RottenPercent < 1f) return VisceralStage.Corpse;
                //Jesus christ thats not even a pawn
                if (ticksLeft > 0) return VisceralStage.Horror;
                //Welp, now we got these
                return VisceralStage.Visceroid;
            }

            return VisceralStage.Empty;
        }
    }

    private float RottenPercent
    {
        get
        {
            if (InnerContainer[0] is Corpse c)
            {
                var rot = c.GetComp<CompRottable>();
                return rot.RotProgress / (rot.PropsRot.TicksToDessicated + rot.PropsRot.TicksToRotStart);
            }

            return 0f;
        }
    }

    public bool ShouldOpen => ticksLeft <= 0;
    public bool CanOpen => InnerContainer.Count > 0 && !hatched;
    public override bool[] DrawBools => new[] { !hatched, !hatched };

    public ThingOwner GetDirectlyHeldThings()
    {
        return InnerContainer;
    }

    public void GetChildHolders(List<IThingHolder> outChildren)
    {
        ThingOwnerUtility.AppendThingHoldersFromThings(outChildren, GetDirectlyHeldThings());
    }

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref pawnSize, "pawnSize");
        Scribe_Values.Look(ref pawnName, "pawnName");
        Scribe_Values.Look(ref kindName, "kindName");
        Scribe_Values.Look(ref prematureHatch, "premature");
        Scribe_Values.Look(ref hatched, "hatched");
        Scribe_Values.Look(ref ticksLeft, "ticksLeft");
        Scribe_Deep.Look(ref InnerContainer, "InnerContainer", this, false, LookMode.Deep);
    }

    public void VisceralSetup(Pawn pawn)
    {
        pawnSize = pawn.BodySize;
        pawnName = pawn.Name?.ToStringShort;
        kindName = pawn.KindLabel;
        InnerContainer = new ThingOwner<Thing>(this, false);
        pawn.DeSpawn();
        InnerContainer.TryAdd(pawn);
        ticksLeft = TRUtils.Range(100000, 120000);
        if (TRUtils.Chance(0.46f))
            prematureHatch = true;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
    }

    public override void Kill(DamageInfo? dinfo = null, Hediff exactCulprit = null)
    {
        if (dinfo.HasValue)
            if (dinfo.Value.Def == DamageDefOf.Flame)
            {
                Destroy(DestroyMode.KillFinalize, true);
                return;
            }

        base.Kill(dinfo, exactCulprit);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        Destroy(mode, false);
    }

    private void Destroy(DestroyMode mode, bool byFire)
    {
        if (!byFire && !hatched)
        {
            HitPoints = MaxHitPoints / 6;
            Open();
            return;
        }

        base.Destroy(mode);
    }

    public override void Tick()
    {
        base.Tick();
        if (hatched || InnerContainer.NullOrEmpty())
            return;

        switch (VisceralStage)
        {
            case VisceralStage.Fresh:
                DoBurnDamage();
                break;
            case VisceralStage.Corpse:
                Rot();
                break;
            case VisceralStage.Horror:
                if (HeldThing is Corpse corpse)
                {
                    var pawn = corpse.InnerPawn;
                    InnerContainer.Clear();
                    InnerContainer.TryAdd(CreateHorror(pawn));
                    if (prematureHatch)
                        Open();
                }

                break;
            case VisceralStage.Visceroid:
                InnerContainer.Clear();
                MakeVisceroids();
                Open();
                break;
        }

        if (ticksLeft > 0)
            ticksLeft--;
    }

    public void Open()
    {
        if (CanOpen)
        {
            hatched = true;
            var dest = Position.RandomAdjacentCell8Way();
            MakeFilth();
            InnerContainer.TryDropAll(dest, Map, ThingPlaceMode.Near);
            Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Things, true, false);
        }
    }

    private void MakeVisceroids()
    {
        var viscerCount = Mathf.RoundToInt(pawnSize);
        for (var i = 0; i < viscerCount; i++)
        {
            var generationRequest = new PawnGenerationRequest(PawnKindDef.Named("Visceroid"));
            var visceral = (Visceroid)PawnGenerator.GeneratePawn(generationRequest);
            visceral.ageTracker = new Pawn_AgeTracker(visceral);
            visceral.Remember(kindName, pawnName);
            InnerContainer.TryAdd(visceral);
        }
    }

    private Pawn_Visceral CreateHorror(Pawn pawn)
    {
        //TODO: Tiberium Creature Faction
        var viscs = Mathf.RoundToInt(pawnSize);
        var request = new PawnGenerationRequest(PawnKindDef.Named("VisceralHorror_Human"));
        if (!pawn.RaceProps.Humanlike)
        {
            if (viscs >= 3f && TRUtils.Chance(0.33f))
                request = new PawnGenerationRequest(PawnKindDef.Named("VisceralBeast"));
            else
                request = new PawnGenerationRequest(PawnKindDef.Named("VisceralHorror_Animal"));
        }

        var visceral = (Pawn_Visceral)PawnGenerator.GeneratePawn(request);
        visceral.ageTracker = new Pawn_AgeTracker(visceral);
        visceral.Remember(kindName, pawnName);
        if (pawn.Faction?.IsPlayer ?? false)
            Messages.Message("TR_VisceralConversion".Translate(pawn.Name.ToStringShort), MessageTypeDefOf.PawnDeath);
        return visceral;
    }

    private void DoBurnDamage()
    {
        var pawn = HeldThing as Pawn;
        if (pawn != null && GenTicks.TicksGame % GenTicks.TickRareInterval == 0)
        {
            float dmg = TRUtils.Range(0, 3);
            var part = pawn.health.hediffSet.GetNotMissingParts().Where(p => p.depth == BodyPartDepth.Outside)
                .RandomElement();
            if (part.coverageAbs > 0)
            {
                //TODO: Work on Tiberium Damages
                if (TRUtils.Chance(0.3f))
                {
                    if (part.IsOrgan())
                    {
                        pawn.health.AddHediff(TRHediffDefOf.ViscousPart, part);
                        return;
                    }

                    if (part.IsLimb() && TRUtils.Chance(0.3f))
                    {
                        pawn.health.AddHediff(TRHediffDefOf.VisceralArm, part);
                        return;
                    }

                    pawn.health.AddHediff(TRHediffDefOf.VisceralBlister, part);
                }
                else
                {
                    var dInfo = new DamageInfo(TRDamageDefOf.TiberiumBurn, dmg, 2, -1, this, part, null,
                        DamageInfo.SourceCategory.ThingOrUnknown, pawn);
                    pawn.TakeDamage(dInfo);
                    if (pawn.apparel != null && !pawn.apparel.WornApparel.NullOrEmpty())
                        pawn.apparel.WornApparel.RandomElement().TakeDamage(dInfo);
                }
            }
        }
    }

    private void Rot()
    {
        if (HeldThing is Corpse corpse)
        {
            var comp = corpse.GetComp<CompRottable>();
            comp.RotProgress += 10;
        }
    }

    private void MakeFilth()
    {
        var cells = GenAdj.CellsAdjacent8Way(this);
        if (RottenPercent < 0.5f)
            for (var i = 0; i < 5; i++)
                FilthMaker.TryMakeFilth(cells.RandomElement(), Map, ThingDefOf.Filth_Slime);

        var filthCount = 12f * Mathf.Clamp01(RottenPercent);
        for (var i = 0; i < filthCount; i++)
        {
            var filth = Rand.Element(ThingDefOf.Filth_CorpseBile, ThingDefOf.Filth_Vomit, ThingDefOf.Filth_Blood);
            FilthMaker.TryMakeFilth(cells.RandomElement(), Map, filth);
        }
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        if (!hatched)
        {
            if (InnerContainer.NullOrEmpty()) return "Empty Container although not hatched - Something is wrong.";
            var append = "";
            append += "TR_VisceralState".Translate() + ": ";
            switch (VisceralStage)
            {
                case VisceralStage.Fresh:
                    append += "Dying".Translate() + " - " +
                              (1 - InnerPawn.health.summaryHealth.SummaryHealthPercent).ToStringPercent();
                    break;
                case VisceralStage.Corpse:
                    append += "Dead".Translate() + " - " + RottenPercent.ToStringPercent() + " " + "Rotten".Translate();
                    break;
                case VisceralStage.Horror:
                    append += "Transmuting".Translate();
                    break;
                case VisceralStage.Visceroid:
                    break;
            }

            if (DebugSettings.godMode)
            {
                sb.AppendLine("[DEBUG]Current Stage: " + VisceralStage);
                sb.AppendLine("[DEBUG]Premature: " + prematureHatch);
            }

            var name = HeldThing.LabelShortCap;
            sb.AppendLine("TR_VisceralContains".Translate() + ": " + name);
            sb.AppendLine(append);
        }

        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        if (DebugSettings.godMode)
        {
            yield return new Command_Action
            {
                defaultLabel = "Debug: Add Random Pawn",
                action = delegate
                {
                    InnerContainer = new ThingOwner<Thing>(this, false);
                    var pawn = PawnGenerator.GeneratePawn(PawnKindDefOf.Colonist);
                    VisceralSetup(pawn);
                    //Open();
                }
            };


            yield return new Command_Action
            {
                defaultLabel = "Debug: Kill",
                action = delegate { InnerPawn.Kill(new DamageInfo(TRDamageDefOf.TiberiumBurn, 999)); }
            };

            yield return new Command_Action
            {
                defaultLabel = "Debug: Open",
                action = delegate { Open(); }
            };
        }
    }
}