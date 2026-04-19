using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using Verse;

namespace TiberiumRim;

public enum PipeMode
{
    Input,
    Output,
    Flow,
    Intersection
}

public class TNW_Pipe : TiberiumNetworkBuilding
{
    //Debug
    private bool debugProduce;
    public TiberiumNetworkBuilding DirectParent;

    //public FluidBox FluidBox;

    public TNW_Pipe from;
    private Rot4 leakDirection = Rot4.Invalid;
    private bool leaking;
    public IntVec3 ParentCell = IntVec3.Invalid;
    public PipeMode pipeMode = PipeMode.Flow;
    public int transferAmount = 6;

    public bool IsIOPipe => pipeMode == PipeMode.Input || pipeMode == PipeMode.Output;

    public override IEnumerable<IntVec3> ConnectableCells => new List<IntVec3> { Position };

    public override Graphic Graphic => TiberiumGraphics.TiberiumNetworkPipes;

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);

        AdaptToAdjacent();
        UpdatePipeMode();
        ConnectedPipes.ForEach(p => p.UpdatePipeMode());
    }

    public void AdaptToAdjacent()
    {
        NetworkMode nMode;
        StoreMode sMode;
        if (!DirectParent.DestroyedOrNull())
        {
            if (StructureSet.Pipes.Any())
            {
                nMode = StructureSet.Pipes.Select(p => p.Network.NetworkMode).FirstOrDefault();
                sMode = StructureSet.Pipes.Select(p => p.Container.mode).FirstOrDefault();
            }
            else
            {
                nMode = DirectParent.Network.NetworkMode;
                sMode = DirectParent.Container.mode;
            }
        }
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        if (!DirectParent.DestroyedOrNull())
            if (DirectParent.def.IOMode != TNWBIOMode.Static)
            {
                DirectParent.InputCells.Remove(Position);
                DirectParent.OutputCells.Remove(Position);
            }

        base.Destroy(mode);
    }

    public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        if (!leaking)
        {
            var chance = 0.15f;
            chance *= Mathf.Lerp(1f, 0f, 1f * dinfo.ArmorPenetrationInt);
            if (HitPoints < MaxHitPoints * 0.75f && Rand.Chance(0.15f))
            {
                leaking = true;
                leakDirection = Rot4.FromAngleFlat(dinfo.Angle);
            }
        }

        base.PostApplyDamage(dinfo, totalDamageDealt);
    }

    public void UpdatePipeMode()
    {
        if (!DirectParent.DestroyedOrNull())
        {
            var mode = DirectParent.def.IOMode;
            switch (mode)
            {
                case TNWBIOMode.Static:
                {
                    if (DirectParent.InputCells.Contains(Position))
                        pipeMode = PipeMode.Input;
                    else if (DirectParent.OutputCells.Contains(Position)) pipeMode = PipeMode.Output;
                    return;
                }
                case TNWBIOMode.Dynamic:
                {
                    if (DirectParent.InputCells.Any())
                        SetMode(PipeMode.Output);
                    else
                        SetMode(PipeMode.Input);
                    return;
                }
                case TNWBIOMode.ForceInput:
                {
                    SetMode(PipeMode.Input);
                    return;
                }
                case TNWBIOMode.ForceOutput:
                {
                    SetMode(PipeMode.Output);
                    return;
                }
            }
        }
        else if (ConnectedPipes.Count == 2)
        {
            pipeMode = PipeMode.Flow;
        }
        else if (ConnectedPipes.Count > 2)
        {
            pipeMode = PipeMode.Intersection;
        }
    }

    public void SetMode(PipeMode mode)
    {
        pipeMode = mode;
        switch (mode)
        {
            case PipeMode.Flow:
            case PipeMode.Intersection:
                break;
            case PipeMode.Input:
            {
                if (!DirectParent.InputCells.Contains(Position))
                {
                    DirectParent.InputCells.Add(Position);
                    DirectParent.OutputCells.Remove(Position);
                }

                return;
            }
            case PipeMode.Output:
            {
                if (!DirectParent.OutputCells.Contains(Position))
                {
                    DirectParent.OutputCells.Add(Position);
                    DirectParent.InputCells.Remove(Position);
                }

                return;
            }
        }
    }

    public void UpdateDirectParent(TiberiumNetworkBuilding newParent, IntVec3 pos)
    {
        DirectParent = newParent;
        ParentCell = pos;
    }

    public void Notify_DirectParentGone(TiberiumNetworkBuilding tnwb)
    {
        if (DirectParent == tnwb)
        {
            StructureSet.RemoveStructure(tnwb);
            DirectParent = null;
            ParentCell = IntVec3.Invalid;
        }
    }

    public override void Tick()
    {
        //base.Tick();
        if (leaking && Container.StoredPct > 0)
        {
            if (HitPoints == MaxHitPoints)
            {
                leaking = false;
            }
            else
            {
                var color = Container.Color;
                TRUtils.ThrowTiberiumLeak(Map, Position, leakDirection, color);
                foreach (TiberiumType type in Container.AllStoredTypes) Container.RemoveValue(type, 1, out var removed);
            }
        }

        if (debugProduce) Container.TryAddValue(TiberiumType.Green, 20, out var excess);
        if (Find.TickManager.TicksGame % 5 == 0 && !Container.Empty)
        {
            if (!DirectParent.DestroyedOrNull())
            {
                if (pipeMode == PipeMode.Input)
                    foreach (TiberiumType type in Container.AllStoredTypes)
                        Container.TryTransferTo(DirectParent.Container, type, 5);

                if (pipeMode == PipeMode.Output)
                    foreach (TiberiumType type in DirectParent.Container.AllStoredTypes)
                        DirectParent.Container.TryTransferTo(Container, type, 5);
            }

            foreach (TNW_Pipe pipe in ConnectedPipes)
                if (pipe != from && Container.GetTotalStorage >= 2)
                {
                    var num = ThroughPut(pipe.Container);
                    if (num > 0)
                    {
                        pipe.from = this;
                        var value = num / Container.AllStoredTypes.Count;
                        foreach (TiberiumType type in Container.AllStoredTypes)
                            Container.TryTransferTo(pipe.Container, type, value);
                    }
                }
            //from = null;
        }
    }

    public int ThroughPut(TiberiumContainer other)
    {
        var val = (Container.GetTotalStorage - other.GetTotalStorage) / 2;
        if (Container.StoredPct != other.StoredPct) val = val > 0 ? val : 1;
        return val;
    }

    public override void DrawGUIOverlay()
    {
        base.DrawGUIOverlay();
        if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && TiberiumRimSettings.settings.ShowNetworkValues)
        {
            Vector3 v = GenMapUI.LabelDrawPosFor(Position);
            var v2 = v + new Vector3(0, 15f, 0);
            var v3 = v - new Vector3(0, 15f, 0);
            GenMapUI.DrawThingLabel(v3, Container.GetTotalStorage.ToString(), Color.green);
            GenMapUI.DrawThingLabel(v, Container.StoredPct.ToStringPercent(), Color.white);
            GenMapUI.DrawThingLabel(v2, Container.Pressure + "p", Color.yellow);
        }
    }

    public override void Draw()
    {
        base.Draw();
        GenDraw.DrawFieldEdges(new List<IntVec3> { ParentCell }, Color.cyan);
        if (Container.StoredPct > 0f)
        {
            var color = Container.Color;
            TiberiumGraphics.TiberiumNetworkPipesGlow.ColoredVersion(ShaderDatabase.MoteGlow, color, color)
                .Draw(DrawPos + Altitudes.AltIncVect, Rotation, this);
        }

        if (Find.Selector.IsSelected(this)) GenDraw.DrawTargetHighlight(from);
    }

    public override string GetInspectString()
    {
        var sb = new StringBuilder();
        sb.AppendLine(base.GetInspectString().TrimEndNewlines());
        if (DebugSettings.godMode)
        {
            sb.AppendLine("PipeMode: " + pipeMode);
            sb.AppendLine("DirectParent: " + DirectParent);
        }

        return sb.ToString().TrimStart().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (var g in base.GetGizmos()) yield return g;

        if (!DirectParent.DestroyedOrNull() && DirectParent.def.IOMode == TNWBIOMode.Dynamic)
            yield return new Command_Action
            {
                defaultLabel = "Switch Mode",
                action = delegate
                {
                    if (pipeMode == PipeMode.Input)
                        SetMode(PipeMode.Output);
                    else
                        SetMode(PipeMode.Input);
                }
            };

        #region Big Container Func

        yield return new Command_Action
        {
            defaultLabel = "ContainerSwitch_TR".Translate(),
            icon = ContentFinder<Texture2D>.Get("UI/Icons/Network/ContainMode_Storage"),
            action = delegate
            {
                var list = new List<FloatMenuOption>();

                list.Add(new FloatMenuOption("SetRGB", delegate
                {
                    var des = new Designator_SetStorageMode();
                    des.mode = StoreMode.RGB;
                    des.ProcessInput(null);
                    //Container.mode = StoreMode.RGB;
                    UpdateConnections();
                }));
                list.Add(new FloatMenuOption("SetRed", delegate
                {
                    Container.mode = StoreMode.Red;
                    UpdateConnections();
                }));
                list.Add(new FloatMenuOption("SetGreen", delegate
                {
                    Container.mode = StoreMode.Green;
                    UpdateConnections();
                }));
                list.Add(new FloatMenuOption("SetBlue", delegate
                {
                    Container.mode = StoreMode.Blue;
                    UpdateConnections();
                }));
                list.Add(new FloatMenuOption("SetGas", delegate
                {
                    Container.mode = StoreMode.Gas;
                    UpdateConnections();
                }));
                list.Add(new FloatMenuOption("SetSludge", delegate
                {
                    Container.mode = StoreMode.Sludge;
                    UpdateConnections();
                }));
                var menu = new FloatMenu(list);
                menu.vanishIfMouseDistant = true;
                Find.WindowStack.Add(menu);
            }
        };

        #endregion

        if (DebugSettings.godMode)
            yield return new Command_Action
            {
                defaultLabel = "Switch Produce",
                action = delegate { debugProduce = !debugProduce; }
            };
    }
}

/*
public class TNW_Pipe2 : TiberiumNetworkBuilding
{
    [Unsaved]
    public TNW_Pipe2 lastPusher;

    private Rot4 leakDirection = Rot4.Invalid;
    private int tickOffset = 0;
    private bool leaking = false;
    private bool debugProduce = false;
    private float transferAmt = 1f;
    private PipeMode PipeMode = PipeMode.Flow;

    public override void ExposeData()
    {
        base.ExposeData();
    }

    public override void SpawnSetup(Map map, bool respawningAfterLoad)
    {
        base.SpawnSetup(map, respawningAfterLoad);
        tickOffset = new IntRange(0, 10).RandomInRange;
        if (!ConnectedStructures.All(t => t is TNW_Pipe))
        {
            PipeMode = PipeMode.Entry;
        }
    }

    public override void Destroy(DestroyMode mode = DestroyMode.Vanish)
    {
        base.Destroy(mode);
    }

    public override void PostApplyDamage(DamageInfo dinfo, float totalDamageDealt)
    {
        float chance = 0.15f;
        chance *= Mathf.Lerp(1f, 0f, 1f * dinfo.ArmorPenetrationInt);
        if(HitPoints < ((float)MaxHitPoints *0.75f) && Rand.Chance(0.15f))
        {
            leaking = true;
            leakDirection = Rot4.FromAngleFlat(dinfo.Angle);
        }
        base.PostApplyDamage(dinfo, totalDamageDealt);
    }

    public override void Tick()
    {
        if (leaking && Container.StoredPct > 0)
        {
            if (HitPoints == MaxHitPoints)
            {
                leaking = false;
            }
            else
            {
                Color color = Container.Color;
                color.a = 1f;
                TRUtils.ThrowTiberiumLeak(Map, Position, leakDirection, color);
            }
        }
        if (Find.TickManager.TicksGame % 100 == 0)
        {
            if (debugProduce)
            {
                Container.TryAddValue(TiberiumType.Green, 5f, out float excess);
            }
            if (leaking && Container.StoredPct > 0)
            {
                Vector2 dir = leakDirection.AsVector2;
                IntVec3 cell = Position + new IntVec3((int)dir.x, 0, (int)dir.y);
                FilthMaker.MakeFilth(cell, Map, TiberiumDefOf.FilthTibLiquid);
                TiberiumLeak leak = (TiberiumLeak)cell.GetThingList(Map).Find(t => t is TiberiumLeak);
                leak?.Setup(Container.Color);
                foreach (TiberiumType type in Container.AllStoredTypes)
                {
                    Container.RemoveValue(type, 2.33f, out float removed);
                }
            }
            if (ConnectedPipes.All(p => p.DestroyedOrNull()))
            {
                return;
            }
            //ConnectedPipes.Shuffle();
            float value = transferAmt / (float)Container.AllStoredTypes.Count;
            //float max = ConnectedPipes.Max(p => p.Container.StoredPct);
            bool hasPusher = ConnectedPipes.Any(p => Container. StoredPct >= Container.StoredPct);
            bool isPusher = ConnectedPipes.Any(p => p.Container.StoredPct < Container.StoredPct) && Container.GetTotalStorage > transferAmt;
            foreach(TiberiumNetworkBuilding b in ConnectedStructures)
            {
                if ((hasPusher || isPusher) && b != lastPusher)
                {
                    if (Container.GetTotalStorage > 0 && b.Container.StoredPct < Container.StoredPct)
                    {
                        foreach (TiberiumType type in Container.AllStoredTypes)
                        {
                            if (Container.TryTransferTo(b.Container, type, value))
                            {
                                if (b is TNW_Pipe2)
                                {
                                    (b as TNW_Pipe2).lastPusher = this;
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    public float GeneralConcentration
    {
        get
        {
            float total = this.Container.GetTotalStorage + ConnectedPipes.Sum(p => p.Container.GetTotalStorage);
            float max = (ConnectedPipes.Count() + 1) * 20f;
            return total / max;
        }
    }

    private void CondLog(string s)
    {
        if (Find.Selector.IsSelected(this))
        {
            Log.Message(s);
        }
    }

    public override Graphic Graphic
    {
        get
        {
          return TiberiumGraphics.TiberiumNetworkPipes;
        }
    }

    public override void DrawGUIOverlay()
    {
        base.DrawGUIOverlay();
        if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest && TiberiumRimSettings.settings.ShowNetworkValues)
        {
            Vector3 v = GenMapUI.LabelDrawPosFor(Position);
            Vector3 v2 = v + new Vector3(0, 15f, 0);
            GenMapUI.DrawThingLabel(v, Container.GetTotalStorage.ToString(), Color.white);
            GenMapUI.DrawThingLabel(v2, GeneralConcentration.ToStringPercent(), Color.yellow);
        }
    }

    public override void Draw()
    {
        base.Draw();
        if (Container.StoredPct > 0f)
        {
            Color color = Container.Color;
            TiberiumGraphics.TiberiumNetworkPipesGlow.ColoredVersion(ShaderDatabase.MoteGlow, color, color).Draw(this.DrawPos + Altitudes.AltIncVect, Rotation, this);
        }
    }

    public override string GetInspectString()
    {
        StringBuilder sb = new StringBuilder();
        if (leaking)
        {
            sb.AppendLine("Leaking!");
        }
        sb.AppendLine("Pipes: ");
        foreach (TNW_Pipe p in ConnectedPipes)
        {
            if (p.Spawned)
            {
                sb.AppendLine(p.ToString());
            }
        }
        return sb.ToString().TrimEndNewlines();
    }

    public override IEnumerable<Gizmo> GetGizmos()
    {
        foreach (Gizmo g in base.GetGizmos())
        {
            yield return g;
        }

        if (ConnectedStructures.Any(p => !(p is TNW_Pipe)))
        {
            yield return new Command_Action
            {
                defaultLabel = "SetInput_TR".Translate(),
                icon = TexCommand.DesirePower,
                action = delegate
                {

                }
            };
        }

        yield return new Designator_Build(TiberiumDefOf.TiberiumPipe);

        yield return new Command_Action
        {
            defaultLabel = "Switch Produce",
            action = delegate
            {
                debugProduce = !debugProduce;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "Switch Leak",
            action = delegate
            {
                leaking = !leaking;
                leakDirection = Rot4.Random;
            }
        };

        yield return new Command_Action
        {
            defaultLabel = "DEBUG: Container Options",
            icon = ContentFinder<Texture2D>.Get("UI/Icons/Network/ContainMode_Storage"),
            action = delegate
            {
                List<FloatMenuOption> list = new List<FloatMenuOption>();
                list.Add(new FloatMenuOption("Add RGB", delegate ()
                {
                    Container.TryAddValue(TiberiumType.Red, 6.3f, out float ex);
                    Container.TryAddValue(TiberiumType.Blue, 6.3f, out ex);
                    Container.TryAddValue(TiberiumType.Green, 6.3f, out ex);
                }));
                list.Add(new FloatMenuOption("Add Green", delegate ()
                {
                    Container.TryAddValue(TiberiumType.Green, 5, out float ex);
                }));
                list.Add(new FloatMenuOption("Add Blue", delegate ()
                {
                    Container.TryAddValue(TiberiumType.Blue, 5, out float ex);
                }));
                list.Add(new FloatMenuOption("Add Red", delegate ()
                {
                    Container.TryAddValue(TiberiumType.Red, 5, out float ex);
                }));
                list.Add(new FloatMenuOption("Clear", delegate ()
                {
                    Container.Clear();
                }));
                FloatMenu menu = new FloatMenu(list);
                menu.vanishIfMouseDistant = true;
                Find.WindowStack.Add(menu);
            }
        };
    }
}
*/