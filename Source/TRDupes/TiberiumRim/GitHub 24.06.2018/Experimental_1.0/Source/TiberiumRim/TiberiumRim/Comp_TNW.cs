using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RimWorld;
using Verse;
using System.Collections;
using UnityEngine;

namespace TiberiumRim
{
    public class Comp_TNW : ThingComp
    {
        public Building_Connector Connector;

        public new CompProperties_TNW props;

        public TiberiumContainer Container;

        public CompGlower compGlower;

        public CompPowerTrader compPower;

        public CompFlickable compFlick;

        private IntVec3 savedPos;

        private int ticksToDoWork;

        private int ticksToPower;

        private bool isSludged = false;

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
            this.props = base.props as CompProperties_TNW;
            compGlower = this.parent.TryGetComp<CompGlower>();
            compPower = this.parent.TryGetComp<CompPowerTrader>();
            compFlick = this.parent.TryGetComp<CompFlickable>();

            if (Container != null)
            {
                Container.parent = this;
            }
            if (!respawningAfterLoad)
            {
                savedPos = new IntVec3(parent.Position.x, parent.Position.y, parent.Position.z);
                CellRect c = parent.OccupiedRect();
                Container = new TiberiumContainer(props.maxStorage, this);
                ResetWorkTimer();
            }
        }

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Values.Look<int>(ref ticksToDoWork, "ticksToProduce");
            Scribe_Values.Look<int>(ref ticksToPower, "ticksToPower");
            Scribe_Values.Look<bool>(ref isSludged, "isSludged");
            Scribe_Values.Look<IntVec3>(ref savedPos, "savedPos");
            Scribe_Deep.Look<TiberiumContainer>(ref Container, "TiberiumContainer");
            Scribe_References.Look<Building_Connector>(ref Connector, "Connector");
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            ticksToPower = 0;
            if (Connector != null && !Connector.Destroyed)
            {
                this.Connector.Destroy(DestroyMode.Deconstruct);
            }
            if (mode == DestroyMode.KillFinalize)
            {
                LeakTiberium(savedPos, previousMap);
            }
            Container = null;
            compGlower?.UpdateLit(previousMap);
            base.PostDestroy(mode, previousMap);
            //TODO: Add new item if container not empty
        }

        public override void CompTick()
        {
            base.CompTick();
            if(Find.TickManager.TicksGame % GenTicks.TickRareInterval == 0)
            {
                if (compGlower != null)
                {
                    compGlower.ReceiveCompSignal("Refueled");
                }
            }
            if (NeedsPower)
            {
                if (!compPower.PowerOn)
                {
                    return;
                }              
            }
            if (compFlick != null)
            {
                if (!compFlick.SwitchIsOn)
                {
                    return;
                }
            }
            if (props.isAutoProducer)
            {
                if (ticksToDoWork <= 0)
                {
                    if (Container.AddCrystal((this.parent as Building_TiberiumSpike).geyser.tiberiumType, props.produceAmt, out float f))
                    {
                        ResetWorkTimer();
                    }
                }
                else { ticksToDoWork -= 1; }
            }           
            if (props.isConsumer)
            {
                if (ticksToPower <= 0)
                {
                    float pct = 0f;

                    if (Container.GetTotalStorage > 0 && Container.MainType != TiberiumType.None)
                    {
                        float value = props.consumeAmtPerDay;
                        float flt = -(Container.GetTotalStorage - props.consumeAmtPerDay);
                        if (flt > 0)
                        {
                            value = props.consumeAmtPerDay - flt;
                        }
                        pct = value / props.consumeAmtPerDay;
                        Container.RemoveCrystal(Container.MainType, value);
                    }
                    else { compPower.PowerOutput = 0f; }

                    ResetPowerTimer((int)(GenDate.TicksPerDay * pct));

                    compGlower.ReceiveCompSignal("Refueled");
                }
                else { ticksToPower -= 1; }
            }
            if (this.Connector != null)
            {
                if (NetworkIsFunctional)
                {
                    if (props.isGlobalStorage)
                    {
                        if (Container.GetTotalStorage > 0f)
                        {
                            if (isSludged)
                            {
                                if (Container.ShouldEmptyUnallowedTypes)
                                {
                                    List<TiberiumType> types = this.Container.GetTypes.Where((TiberiumType x) => x != TiberiumType.Sludge).ToList();
                                    TiberiumType type = types.RandomElement();
                                    List<Comp_TNW> compList = Connector.Network.AllGlobalStorages.FindAll((Comp_TNW x) => !x.Container.CapacityFull && !x.props.isLocalStorage && x.Container.AcceptsType(type));
                                    for (int i = 0; i < compList.Count; i++)
                                    {
                                        Comp_TNW comp = compList[i];
                                        TransferTib(Container, comp.Container, 1f, type);
                                    }
                                }
                            }
                            else
                            {
                                if (Container.ShouldUnloadSludgeInContainer(out TiberiumContainer sludger))
                                {
                                    TransferTib(Container, sludger, 1f, TiberiumType.Sludge);
                                }
                            }
                        }
                    }
                    if (props.isLocalStorage)
                    {
                        if (props.isConsumer)
                        {
                            if (!Container.CapacityFull)
                            {
                                List<Comp_TNW> compList = Connector.Network.AllGlobalStorages.FindAll((Comp_TNW x) => x.Container.GetTotalStorage >= 0 && !x.props.isConsumer && x.Container.MainType != TiberiumType.Sludge);
                                for (int i = 0; i < compList.Count; i++)
                                {
                                    Comp_TNW comp = compList[i];
                                    if (comp.Container.MainType != TiberiumType.None)
                                    {
                                        TransferTib(comp.Container, Container, 1f);
                                    }

                                }
                            }
                            return;
                        }
                        if (Container.GetTotalStorage > 0)
                        {                          
                            List<Comp_TNW> compList = Connector.Network.AllGlobalStorages.FindAll((Comp_TNW x) => !x.Container.CapacityFull && !x.props.isLocalStorage && x.Container.AcceptsType(this.Container.MainType));
                            for (int i = 0; i < compList.Count; i++)
                            {
                                if (Container.MainType != TiberiumType.None)
                                {
                                    Comp_TNW comp = compList[i];
                                    TransferTib(Container, comp.Container, 1f);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void TransferTib(TiberiumContainer containerFrom, TiberiumContainer containerTo, float value, TiberiumType specialType = TiberiumType.None)
        {
            TiberiumType type = specialType;
            if (specialType == TiberiumType.None)
            {
                type = containerFrom.MainType;
            }
            float value2 = containerFrom.ValueForType(type) < value ? containerFrom.ValueForType(type) : value;
            if (containerTo.AddCrystal(type, value2, out float excess))
            {
                containerFrom.RemoveCrystal(type, value2 - excess);
            }
        }

        public void ResetWorkTimer()
        {
            this.ticksToDoWork = GenTicks.SecondsToTicks(this.props.workTime);
        }

        public void ResetPowerTimer(int ticks)
        {
            this.ticksToPower = ticks;            
        }

        public bool IsStorage
        {
            get
            {
                return props.isGlobalStorage || props.isLocalStorage;
            }
        }

        public bool IsGeneratingPower
        {
            get
            {
                return this.ticksToPower > 0;
            }
        }

        public bool ConnectedToNetwork
        {
            get
            {
                if (Connector != null)
                {
                    if (Connector.Network != null)
                    {

                        return Connector?.Network != null;
                    }
                }
                return false;
            }
        }

        public bool NetworkIsFunctional
        {
            get
            {
                return Connector.Network != null && !Connector.Network.IsBrokenDown() && NetworkIsPowered;
            }
        }

        public bool NetworkIsPowered
        {
            get
            {
                if (ConnectedToNetwork)
                {
                    CompPowerTrader compPower = this.Connector.Network.TryGetComp<CompPowerTrader>();
                    if(compPower != null && compPower.PowerOn)
                    {
                        return true;
                    }
                }
                return false;
            }
        }

        public bool NeedsPower => this.compPower != null;

        public void LeakTiberium(IntVec3 parentPos, Map map)
        {
            List<TiberiumCrystal> tibList = new List<TiberiumCrystal>();
            foreach (TiberiumType type in Container.GetTypes)
            {
                TiberiumCrystalDef crystalDef = TRUtils.CrystalDefFromType(type);
                float tibAmt = Container.ValueForType(type) / crystalDef.tiberium.maxHarvestValue;
                float scale = tibAmt / MainTCD.MainTiberiumControlDef.TiberiumLeakScale;
                for (int i = 0; i < scale; i++)
                {
                    if (crystalDef != null)
                    {
                        TiberiumCrystal newCrystal = (TiberiumCrystal)ThingMaker.MakeThing(crystalDef);
                        newCrystal.def = crystalDef;
                        tibList.Add(newCrystal);
                    }
                }
            }
            List<IntVec3> spawnCells = FindPositions(tibList, parentPos, map);
            for (int i = 0; i < tibList.Count; i++)
            {
                if (tibList[i] != null)
                {
                    if (Rand.Chance(0.5f))
                    {
                        GenSpawn.Spawn(LiquidFromType(tibList[i].def.TibType), spawnCells[i], map);
                    }
                    GenTiberiumReproduction.SetTiberiumTerrainAndType(tibList[i].def, spawnCells[i].GetTerrain(map), out TiberiumCrystalDef crystal, out TerrainDef terrain); 
                    if(crystal == null)
                    {
                        crystal = tibList[i].def;
                    }
                    GenSpawn.Spawn(crystal, spawnCells[i], map);
                }
            }
        }

        public ThingDef LiquidFromType(TiberiumType type)
        {
            ThingDef def = null;
            if(type == TiberiumType.Green || type == TiberiumType.Sludge)
            {
                def = DefDatabase<ThingDef>.GetNamed("FilthTibLiquidGreen");
            }
            if (type == TiberiumType.Blue)
            {
                def = DefDatabase<ThingDef>.GetNamed("FilthTibLiquidBlue");
            }
            if (type == TiberiumType.Red)
            {
                def = DefDatabase<ThingDef>.GetNamed("FilthTibLiquidRed");
            }
            return def;
        }

        public List<IntVec3> FindPositions(List<TiberiumCrystal> tibList, IntVec3 origin, Map map)
        {
            List<IntVec3> Positions = new List<IntVec3>();
            Positions.Add(origin);
            int cells = 0;
            while (cells < tibList.Count)
            {
                Room room = origin.GetRoom(map);
                cells += room.CellCount - cells;
                if (room.CellCount < tibList.Count)
                {
                    Building building = room.BorderCells.RandomElement().GetFirstBuilding(map);
                    if (building != null)
                    {
                        building.Destroy();
                    }
                }
            }
            while (Positions.Count < tibList.Count)
            {
                List<IntVec3> checkCells = Positions.FindAll((IntVec3 x) => x.CellsAdjacent8Way().Where((IntVec3 y) => !Positions.Contains(y)).Count() > 0);
                for (int i = 0; i < checkCells.Count; i++)
                {                    
                    IntVec3 c = checkCells[i];
                    HashSet<IntVec3> list = new HashSet<IntVec3>();
                    list.AddRange(c.CellsAdjacent8Way().Where((IntVec3 y) => y.InBounds(map) && (y.GetFirstBuilding(map) != null ? y.GetFirstBuilding(map).def.terrainAffordanceNeeded == TerrainAffordance.Light && y.GetFirstBuilding(map).def.thingClass != typeof(Mineable) : true) && !Positions.Contains(y)).ToList());
                    if (list.Count > 0)
                    {
                        IntVec3 cell = list.RandomElement();
                        Positions.Add(cell);
                    }
                }
            }
            return Positions;
        }

        public override IEnumerable<Gizmo> CompGetGizmosExtra()
        {
            if (props.isGlobalStorage && !props.isLocalStorage)
            {
                Command_Action ContainMode = new Command_Action
                {
                    defaultLabel = !isSludged ? "StorageMode".Translate() : "SludgeMode".Translate(),
                    defaultDesc = "ContainModeDesc".Translate(),
                    icon = isSludged ? ContentFinder<Texture2D>.Get("UI/Icons/ContainMode_Sludge", true) : ContentFinder<Texture2D>.Get("UI/Icons/ContainMode_Storage", true),
                    hotKey = KeyBindingDefOf.Misc1,
                    action = delegate
                    {
                        if (!isSludged)
                        {
                            this.Container.SetAllowedType(TiberiumType.Sludge);
                            isSludged = !isSludged;
                        }
                        else { this.Container.SetAllowedType(TiberiumType.None); isSludged = !isSludged; }
                    }
                };
                yield return ContainMode;
            }

            if (Prefs.DevMode)
            {
                yield return new Command_Action
                {
                    defaultLabel = "Fill Slduge",
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        if (this.Container.AddCrystal(TiberiumType.Sludge, 500f, out float f))
                        { }
                    }
                };
                yield return new Command_Action
                {
                    defaultLabel = "Fill Green",
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        if(this.Container.AddCrystal(TiberiumType.Green, 500f, out float f))
                        {}
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "Fill Blue",
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        if (this.Container.AddCrystal(TiberiumType.Blue, 500f, out float f))
                        { }
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "Fill Red",
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        if (this.Container.AddCrystal(TiberiumType.Red, 500f, out float f))
                        { }
                    }
                };

                yield return new Command_Action
                {
                    defaultLabel = "+100 credits",
                    icon = TexCommand.DesirePower,
                    action = delegate
                    {
                        if(this.Container.AddCrystal(Rand.Element(TiberiumType.Green, TiberiumType.Blue, TiberiumType.Red), 100f, out float f))
                        {}
                    }
                };
            }
            yield break;
        }

        public override void PostDraw()
        {
            base.PostDraw();
            if (Find.Selector.IsSelected(this.parent))
            {
                if (Connector != null)
                {
                    List<IntVec3> pos = new List<IntVec3>
                    {
                        Connector.Position
                    };
                    GenDraw.DrawFieldEdges(pos);
                }
            }
        }

        public override string CompInspectStringExtra()
        {
            StringBuilder stringBuilder = new StringBuilder();
            if (!ConnectedToNetwork)
            {
                stringBuilder.AppendLine("MissingNetwork".Translate());
            }
            if (isSludged)
            {
                stringBuilder.AppendLine("IsSludged".Translate());
            }
            if (props.isLocalStorage || props.isGlobalStorage)
            {
                stringBuilder.AppendLine("StoredTib".Translate() + ": " + Math.Round(this.Container.GetTotalStorage, 0));
            }
            if (props.isConsumer)
            {
                stringBuilder.AppendLine("Consumes".Translate(new object[] {
                    props.consumeAmtPerDay
                }));
                if (this.ticksToPower > 0)
                {
                    stringBuilder.AppendLine("DaysRemaining".Translate(new object[]
                    {
                    Math.Round(GenDate.TicksToDays(this.ticksToPower), 2)
                    }));
                }
            }
            return stringBuilder.ToString().TrimEndNewlines();
        }
    }

    public class CompProperties_TNW : CompProperties
    {       
        public bool isGlobalStorage = false;

        public bool isLocalStorage = false;

        public bool isAutoProducer = false;

        public bool isConsumer = false;

        public int consumeAmtPerDay = 0;

        public float maxStorage = 0f;

        public float produceAmt = 0f;

        public int workTime = 6;

        public CompProperties_TNW()
        {
            this.compClass = typeof(Comp_TNW);
        }
    }
}
