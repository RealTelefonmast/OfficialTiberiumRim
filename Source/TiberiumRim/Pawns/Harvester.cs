using System.Collections;
using System.Collections.Generic;
using System.Text;
using RimWorld;
using TiberiumRim;
using TR.Networks.TiberiumNetwork;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;
using Verse.AI;

namespace TR;

public enum HarvestMode
{
    All,        // _2/_3
    Nearest,
    Value,
    Moss
}

public enum HarvesterPriority
{
    Drafted,    // current — more expressive than _2/_3's None
    Harvest,
    Unload,
    Idle
}

public class HarvesterKindDef : MechanicalPawnKindDef
{
    public List<TiberiumValueType> acceptedTypes = new();
    public float explosionRadius = 7;
    public float harvestValue = 0.125f;     // current
    public int maxStorage;
    public float unloadValue = 0.125f;
    public ThingDef wreckDef;
}

[StaticConstructorOnStartup]    // _2/_3
public class Harvester : MechanicalPawn, IContainerHolder
{
    // --- Fields ---

    protected TiberiumContainer container;

    // Search/harvest settings
    private int failedTibSearches;
    private bool forceReturn;
    private HarvestMode harvestMode = HarvestMode.Nearest;
    private int idleTicksLeft;
    public bool idleAtRefinery;                         // _3
    private IntVec3 idlePosition = IntVec3.Invalid;     // _3

    // Refinery tracking
    private CompTNW_Refinery mainRefinery;  // _2

    // Preferences
    private IntVec3 lastKnownPos;
    private TiberiumProducer preferredProducer;
    private TiberiumCrystalDef preferredType;
    public TiberiumCrystalDef TiberiumDefToPrefer;      // _3

    // Waiting
    private int waitingTicks = -1;  // _2

    // --- Kind def ---

    public new HarvesterKindDef kindDef => (HarvesterKindDef)base.kindDef;

    // --- Refinery properties ---

    public CompTNS_Refinery RefineryComp { get; private set; }

    public Building Refinery
    {
        get => ParentBuilding;
        set => ParentBuilding = value;
    }

    public bool AtRefinery => Position == Refinery?.InteractionCell;

    // _3 — MainRefinery property wrapping mainRefinery field with fallback
    public TNW_Refinery MainRefinery
    {
        get
        {
            if (!mainRefinery.DestroyedOrNull()) return mainRefinery;
            return AvailableRefinery;
        }
        set => mainRefinery = value;
    }

    // _3 — find a non-full refinery for unloading
    public TNW_Refinery RefineryToUnload
    {
        get
        {
            if (!mainRefinery.DestroyedOrNull())
                if (!mainRefinery.Container.CapacityFull)
                    return mainRefinery;
            return TiberiumNet.AllRefineries.Find(r => !r.DestroyedOrNull() && !r.Container.CapacityFull);
        }
    }

    // _2 — comp-based current refinery with fallback
    public CompTNW_Refinery CurrentRefinery => mainRefinery.CanBeRefinedAt ? mainRefinery : AvailableRefinery;

    // _2 — comp-based available refinery search
    private CompTNW_Refinery AvailableRefinery
    {
        get { return TNWManager.MainStructureSet.Refineries.Find(r => r.CanBeRefinedAt); }
    }

    // --- Idle position ---

    public IntVec3 IdlePos => RefineryComp?.PositionFor(this) ?? lastKnownPos;

    // --- Mode / preferences ---

    public HarvestMode HarvestMode
    {
        get => harvestMode;
        private set => harvestMode = value;
    }

    public TiberiumProducer PreferredProducer
    {
        get => preferredProducer;
        private set => preferredProducer = value;
    }

    public TiberiumCrystalDef PreferredType
    {
        get => preferredType;
        private set => preferredType = value;
    }

    // --- Return / force ---

    public bool ForceReturn
    {
        get => forceReturn;
        private set => forceReturn = value;
    }

    // _2 — includes recallHarvesters signal from refinery comp
    public bool ForcedReturn => forceReturn || mainRefinery.recallHarvesters;

    private bool ShouldReturnToIdle => ForceReturn || RefineryComp.RecallHarvesters;
    public bool PlayerInterrupt => ShouldReturnToIdle || Drafted;

    // --- Waiting ---

    public bool IsWaiting => waitingTicks > 0;  // _2

    // --- Refinery state ---

    // _2 — checks both ParentBuilding and mainRefinery comp
    public bool MainRefineryLost => ParentBuilding.DestroyedOrNull() || mainRefinery == null;

    // --- Tiberium availability ---

    private bool HasAvailableTiberium => HarvestMode == HarvestMode.Moss
        ? TiberiumManager.MossAvailable
        : TiberiumManager.TiberiumAvailable;

    // _3 — explicit crystal-level availability checks
    private bool TiberiumAvailable => TiberiumManager.TiberiumCrystals.Any(c => c.HarvestValue >= 1f);
    private bool MossAvailable => TiberiumManager.TiberiumMoss.Count() > 0;

    // --- Container state ---

    private bool ContainerFull => Container.CapacityFull;

    // --- Harvest target ---

    // _2 getter (reservation manager with validity update); _3 setter preserved
    public TiberiumCrystal HarvestTarget
    {
        get
        {
            if (!TNWManager.ReservationManager.TargetValidFor(this)) TNWManager.ReservationManager.TryUpdate();
            return TNWManager.ReservationManager.Reservations[this];
        }
        set { }
    }

    // _2 — direct reservation lookup without validity update
    public TiberiumCrystal CurrentHarvestTarget => TNWManager.ReservationManager.Reservations[this];

    // --- Priority booleans ---

    public bool ShouldIdle => Container.Empty &&
                               (!HasAvailableTiberium ||
                                (Container.TotalStorage > 0 && RefineryComp.Container.CapacityFull));

    public bool ShouldHarvest => !ContainerFull && HasAvailableTiberium;

    public bool ShouldUnload => ContainerFull || (container.TotalStorage > 0 && !HasAvailableTiberium);

    private bool CanHarvest => !IsUnloading;

    public bool CanUnload => Container.TotalStorage > 0 && RefineryComp.CanBeRefinedAt;

    // _2 — job-def-based unloading check
    public bool Unloading => CurJobDef == TiberiumDefOf.UnloadAtRefinery;

    // --- Job state ---

    public bool IsHarvesting
    {
        get
        {
            if (CurJob == null) return false;
            if (jobs.curDriver is JobDriver_HarvestTiberium) return true;
            return false;
        }
    }

    public bool IsUnloading
    {
        get
        {
            if (CurJob == null) return false;
            if (jobs.curDriver is JobDriver_UnloadAtRefinery) return true;
            return false;
        }
    }

    // --- Priority ---

    public HarvesterPriority CurrentPriority
    {
        get
        {
            if (Drafted) return HarvesterPriority.Drafted;
            if (ShouldReturnToIdle) return HarvesterPriority.Idle;
            if (ShouldHarvest && CanHarvest) return HarvesterPriority.Harvest;
            if (ShouldUnload && CanUnload) return HarvesterPriority.Unload;
            return HarvesterPriority.Idle;
        }
    }

    // --- FX ---

    public override Color[] ColorOverrides => new[] { Container.Color };
    public override float[] OpacityFloats => new[] { Container.StoredPercent };
    public override bool[] DrawBools => new[] { true };

    private Texture2D HarvestModeTexture
    {
        get
        {
            return HarvestMode switch
            {
                HarvestMode.Nearest => TiberiumContent.HarvesterNearest,
                HarvestMode.Value => TiberiumContent.HarvesterValue,
                HarvestMode.Moss => TiberiumContent.HarvesterMoss,
                _ => BaseContent.BadTex
            };
        }
    }

    // --- Container access ---

    public TiberiumContainer Container => container;

    // === IContainerHolder ===

    public void Notify_ContainerFull() { }

    public bool AnyAvailableRefinery(out Building building)
    {
        building = null;
        return false;
    }

    // === Notifications ===

    public void Notify_RefineryDestroyed(CompTNS_Refinery notifier)
    {
        ResolveNewRefinery(notifier);
    }

    public void Notify_CouldNotFindTib()
    {
        failedTibSearches++;
        if (failedTibSearches > 10)
        {
            idleTicksLeft = Mathf.RoundToInt(GenDate.TicksPerHour * 0.75f);
            failedTibSearches = 0;
        }
    }

    // === Lifecycle ===

    public override void ExposeData()
    {
        base.ExposeData();
        Scribe_Values.Look(ref forceReturn, "ForceReturn");
        Scribe_Values.Look(ref harvestMode, "HarvestMode");
        Scribe_References.Look(ref preferredProducer, "prefProducer");
        Scribe_Defs.Look(ref preferredType, "prefType");
        Scribe_Deep.Look(ref container, "tibContainer");
        Scribe_Values.Look(ref lastKnownPos, "lastPos");
        Scribe_References.Look(ref mainRefinery, "mainRefinery");    // _2/_3
        Scribe_Values.Look(ref idleAtRefinery, "idleAtRefinery");    // _3
        Scribe_Values.Look(ref idlePosition, "idlePosition");        // _3

        if (Scribe.mode == LoadSaveMode.PostLoadInit)
        {
            if (Refinery != null)
                RefineryComp = Refinery.GetComp<CompTNS_Refinery>();
            mainRefinery = ParentBuilding?.GetComp<CompTNW_Refinery>();  // _2
        }
    }

    // _3 — pre-spawn refinery assignment
    public void PreSetup(TNW_Refinery refinery)
    {
        mainRefinery = refinery;
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        TNWManager.RegisterHarvester(this);  // _2
        if (!respawningAfterLoad)
        {
            container = new TiberiumContainer(kindDef.maxStorage, kindDef.acceptedTypes, this, this);
            idlePosition = MainRefinery.InteractionCell;  // _3
            if (ParentBuilding == null) ResolveNewRefinery();
        }
    }

    public override void DeSpawn(DestroyMode mode = DestroyMode.Vanish)
    {
        TNWManager.DeregisterHarvester(this);  // _2
        base.DeSpawn(mode);
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
    }

    public override void Kill(DamageInfo? dinfo, Hediff exactCulprit = null)
    {
        base.Kill(dinfo, exactCulprit);
        if (Container.TotalStorage > 0)
        {
            var spawnDef = TRUtils.CrystalDefFromType(Container.MainValueType, out var isGas);
            var radius = kindDef.explosionRadius * Container.StoredPercent;
            var damage = 10 * Container.StoredPercent;
            //TODO: Add Tiberium damagedef
            GenExplosion.DoExplosion(Position, Map, radius, DamageDefOf.Bomb, this, damage, 5, null, null, null, null,
                spawnDef, 0.18f);
        }

        GenSpawn.Spawn(kindDef.wreckDef, Position, Map);
        DeSpawn(DestroyMode.KillFinalize);
    }

    // _2 — tick with waiting countdown and refinery-lost recovery
    public override void Tick()
    {
        base.Tick();
        if (Downed) Kill(null);
        if (waitingTicks > 0) waitingTicks--;
        if (MainRefineryLost) UpdateRefineries();
    }

    public override void PreApplyDamage(ref DamageInfo dinfo, out bool absorbed)
    {
        base.PreApplyDamage(ref dinfo, out absorbed);
        //TODO: EVA Warning
    }

    // === Harvest logic ===

    public bool CanHarvestTiberium(TiberiumCrystalDef crystal)
    {
        return crystal.IsMoss ? HarvestMode == HarvestMode.Moss : HarvestMode != HarvestMode.Moss;
    }

    // _2 — explicit wait requested by job drivers
    public void SetToWait()
    {
        waitingTicks = GenTicks.SecondsToTicks(15);
    }

    // _3 — flood-fill based harvest target search
    public IEnumerator CalculateTiberium()
    {
        Predicate<IntVec3> predicate = x => x.IsValid && x.Standable(Map) && HarvestTarget == null;
        var action = delegate(IntVec3 c)
        {
            if (HarvestTarget == null)
            {
                HarvestTarget = c.GetTiberium(Map);
                if (HarvestTarget.HarvestValue < 1f || !this.CanReserve(HarvestTarget) ||
                    !this.CanReach(c, PathEndMode.ClosestTouch, Danger.Deadly)) HarvestTarget = null;
            }
        };
        Map.floodFiller.FloodFill(Position, predicate, action);
        yield return null;
    }

    // === Refinery management ===

    public void SetMainRefinery(Building building, CompTNS_Refinery refinery, CompTNS_Refinery lastParent)
    {
        if (lastParent != null)
        {
            Refinery = null;
            lastParent.RemoveHarvester(this);
            Messages.Message("TR_HarvesterNewRefinery".Translate(), new LookTargets(building, this),
                MessageTypeDefOf.NeutralEvent);
        }

        lastKnownPos = building.InteractionCell;
        Refinery = building;
        RefineryComp = refinery;
        RefineryComp.AddHarvester(this);
    }

    // _2 — single-building overload
    public void SetMainRefinery(Building building)
    {
        if (ParentBuilding == building) return;
        mainRefinery?.RemoveHarvester(this);
        ParentBuilding = building;
        mainRefinery = building.TryGetComp<CompTNW_Refinery>();
        mainRefinery.AddHarvester(this);
        Messages.Message("TR_HarvesterNewRefinery".Translate(def.LabelCap), new LookTargets(building, this),
            MessageTypeDefOf.NeutralEvent);
    }

    // _3 — direct TNW_Refinery overload
    public void SetMainRefinery(TNW_Refinery refinery)
    {
        mainRefinery = refinery;
        idlePosition = MainRefinery.InteractionCell;
        Messages.Message("NewRefinerySet".Translate(def.LabelCap), refinery, MessageTypeDefOf.NeutralEvent);
    }

    private void ResolveNewRefinery(CompTNS_Refinery lastParent = null)
    {
        if (Map == null) return;
        foreach (var building in Map.listerBuildings.allBuildingsColonist)
        {
            var refinery = building.TryGetComp<CompTNS_Refinery>();
            if (refinery == null || refinery == lastParent) continue;
            SetMainRefinery(building, refinery, lastParent);
            return;
        }
    }

    // _2 — comp-based refinery resolution with ignore list
    public void UpdateRefineries(Building forceMain = null, CompTNW_Refinery toIgnore = null)
    {
        if (forceMain != null)
        {
            SetMainRefinery(forceMain);
            return;
        }

        foreach (CompTNW_Refinery refinery in TNWManager.MainStructureSet.Refineries)
            if (refinery != null && refinery != toIgnore)
                SetMainRefinery((Building)refinery.parent);
    }

    // _3 — direct TNW_Refinery resolution, registers harvester with all refineries
    public void UpdateRefineries(TNW_Refinery forceMain = null)
    {
        foreach (TNW_Refinery refinery in TiberiumNet.AllRefineries)
        {
            if (refinery != null)
            {
                if (!refinery.boundHarvesters.Contains(this)) refinery.boundHarvesters.Add(this);
                if (MainRefineryLost) SetMainRefinery(refinery);
            }

            if (forceMain != null) SetMainRefinery(forceMain);
        }
    }

    // === Draw ===

    public override void Draw()
    {
        base.Draw();
        if (Find.Selector.IsSelected(this) && Find.CameraDriver.CurrentZoom <= CameraZoomRange.Middle)
        {
            var r = default(GenDraw.FillableBarRequest);
            r.center = DrawPos;
            r.center.z += 1.5f;
            r.size = new Vector2(3, 0.15f);
            r.fillPercent = Container.StoredPercent;
            r.filledMat = TiberiumContent.Harvester_FilledBar;
            r.unfilledMat = TiberiumContent.Harvester_EmptyBar;
            r.margin = 0.12f;
            GenDraw.DrawFillableBar(r);
        }
    }

    // === UI ===

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var gizmo in base.GetGizmos()) yield return gizmo;

        if (Faction != Faction.OfPlayer) yield break;

        if (DebugSettings.godMode) { }

        foreach (var g in Container.GetGizmos()) yield return g;

        yield return new Command_Action
        {
            defaultLabel = "TR_HarvesterMode".Translate(),
            defaultDesc = "TR_HarvesterModeDesc".Translate(),
            icon = HarvestModeTexture,
            action = delegate
            {
                var list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("TRHMode_Nearest".Translate(),
                    delegate { harvestMode = HarvestMode.Nearest; }));
                list.Add(new FloatMenuOption("TRHMode_Valuable".Translate(),
                    delegate { harvestMode = HarvestMode.Value; }));
                list.Add(new FloatMenuOption("TRHMode_Moss".Translate(),
                    delegate { harvestMode = HarvestMode.Moss; }));
                var menu = new FloatMenu(list);
                menu.vanishIfMouseDistant = true;
                Find.WindowStack.Add(menu);
            }
        };

        /*
        yield return new Command_Target
        {
            defaultLabel = "TR_ProducerPrefLabel".Translate(),
            defaultDesc = "TR_ProducerPrefDesc".Translate(),
            icon = BaseContent.BadTex,
            action = delegate (Thing thing) { }
        };

        yield return new Command_Action
        {
            defaultLabel = "TR_TypePrefLabel".Translate(),
            defaultDesc = "TR_TypePrefDesc".Translate(),
            icon = BaseContent.BadTex,
        };
        */

        yield return new Command_Action
        {
            defaultLabel = ForceReturn ? "TR_HarvesterHarvest".Translate() : "TR_HarvesterReturn".Translate(),
            defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
            icon = ForceReturn ? TiberiumContent.HarvesterHarvest : TiberiumContent.HarvesterReturn,
            action = delegate
            {
                ForceReturn = !ForceReturn;
                jobs.EndCurrentJob(JobCondition.InterruptForced);
            }
        };

        // _3 — idle-at-refinery toggle (distinct from ForceReturn)
        yield return new Command_Action
        {
            defaultLabel = idleAtRefinery ? "TR_Harvester_Return".Translate() : "TR_Harvester_Harvest".Translate(),
            defaultDesc = "TR_Harvester_ReturnDesc".Translate(),
            icon = idleAtRefinery
                ? ContentFinder<Texture2D>.Get("UI/Icons/Network/Return")
                : ContentFinder<Texture2D>.Get("UI/Icons/Network/Harvest"),
            action = delegate { idleAtRefinery = !idleAtRefinery; }
        };

        yield return new Command_Target
        {
            defaultLabel = "TR_HarvesterRefinery".Translate(),
            defaultDesc = "TR_HarvesterRefineryDesc".Translate(),
            icon = TiberiumContent.HarvesterRefinery,
            targetingParams = RefineryTargetInfo.ForHarvester(),
            action = delegate(Thing thing)
            {
                if (thing == null) return;
                if (thing is Building building)
                {
                    var refinery = thing.TryGetComp<CompTNS_Refinery>();
                    if (refinery != null)
                        SetMainRefinery(building, refinery, RefineryComp);
                }
            }
        };
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendFormat(base.GetInspectString());
        sb.AppendLine();
        if (DebugSettings.godMode)
        {
            sb.AppendLine("##Debug##");
            if (IsWaiting) sb.AppendLine("Waiting: " + waitingTicks.TicksToSeconds());  // _2
            sb.AppendLine("Drafted: " + Drafted);                                        // _2
            sb.AppendLine("Forced Return: " + ForcedReturn);                             // _2
            sb.AppendLine("Unloading: " + Unloading);                                   // _2
            sb.AppendLine("Capacity Full: " + Container.CapacityFull);                  // _2
            sb.AppendLine("Tiberium f/mode Available: " + HasAvailableTiberium);        // _2
            sb.AppendLine("Exact Storage: " + Container.TotalStorage);                  // _2
            sb.AppendLine("Mode: " + harvestMode);                                      // _2
            sb.AppendLine("Current Harvest Target: " + CurrentHarvestTarget);           // _2
            sb.AppendLine("Valid: " + TNWManager.ReservationManager.TargetValidFor(this) +
                          " CanBeHarvested: " + CurrentHarvestTarget?.CanBeHarvestedBy(this) +
                          " Spawned: " + CurrentHarvestTarget?.Spawned +
                          " Destroyed: " + CurrentHarvestTarget?.Destroyed);            // _2
            sb.AppendLine("CurJob: " + CurJob);
            sb.AppendLine("Priority: " + CurrentPriority);
            sb.AppendLine("Has Tiberium: " + HasAvailableTiberium);
            sb.AppendLine("Should Harvest: " + ShouldHarvest);
            sb.AppendLine("Can Harvest: " + CanHarvest);
            sb.AppendLine("Should Unload: " + ShouldUnload);
            sb.AppendLine("Can Unload: " + CanUnload);
            sb.AppendLine("Should Idle: " + ShouldIdle);
            sb.AppendLine("Player Interrupted: " + PlayerInterrupt);
            sb.AppendLine("Is Unloading: " + IsUnloading);
            sb.AppendLine("Is Harvesting: " + IsHarvesting);

            /*
            sb.AppendLine("HarvesterQueue: " + HarvestQueue.Count);
            sb.AppendLine("Contained In Queue:" + HarvestQueue.ToStringSafeEnumerable());
            */
            //sb.AppendLine("Current Harvest Target: " + CurrentHarvestTarget);
            //sb.AppendLine("Valid: " + TNWManager.ReservationManager.TargetValidFor(this) + " CanBeHarvested: " + CurrentHarvestTarget?.CanBeHarvestedBy(this) + " Spawned: " + CurrentHarvestTarget?.Spawned + " Destroyed: " + CurrentHarvestTarget?.Destroyed);
        }

        return sb.ToString().TrimEndNewlines();
    }
}
