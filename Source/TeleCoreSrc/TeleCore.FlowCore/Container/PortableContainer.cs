using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using UnityEngine;
using UnityEngine.UIElements;
using Verse;

namespace TeleCore
{
    /// <summary>
    /// Temporary <see cref="NetworkContainer"/> holder spawned upon deconstruction of a <see cref="Building"/> containing a <see cref="Comp_NetworkStructure"/> comp.
    /// </summary>
    public class PortableContainer : FXThing, IContainerHolder
    {
        private NetworkDef networkDef;
        private NetworkContainer container;
        private ContainerProperties containerProps;

        //Target Request
        private TargetingParameters paramsInt;
        private LocalTargetInfo currentDesignatedTarget = LocalTargetInfo.Invalid;

        public string ContainerTitle => "TELE.PortableContainer.Title".Translate();
        public Thing Thing => this;
        public NetworkContainer Container => container;
        public NetworkDef NetworkDef => networkDef;
        public ContainerProperties ContainerProps => containerProps;
        public float EmptyPercent => Container.StoredPercent - 1f;

        //Target Request
        public LocalTargetInfo TargetToEmptyAt => currentDesignatedTarget;
        public bool HasValidTarget => currentDesignatedTarget.IsValid;

        //FX
        public override bool FX_AffectsLayerAt(int index)
        {
            return true;
        }

        public override bool FX_ShouldDrawAt(int index)
        {
            return true;
        }
        public override float FX_GetOpacityAt(int index)
        {
            return Container.StoredPercent;
        }

        public override Color? FX_GetColorAt(int index)
        {
            return index switch
            {
                0 => Container?.Color ?? Color.white
            };
        }

        public override Vector3? FX_GetDrawPositionAt(int index)
        {
            return DrawPos + Vector3.one;
        }

        //
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Deep.Look(ref containerProps, "containerProps");
            Scribe_Deep.Look(ref container, "networkContainer", this);
        }

        //Setup
        public void SetupProperties(NetworkDef networkDef, NetworkContainer container, ContainerProperties props)
        {
            this.networkDef = networkDef;
            this.container = container;
            this.containerProps = props.Copy();
            this.containerProps.leaveContainer = false;
        }

        //
        public virtual void Notify_ContainerFull() { }
        public virtual void Notify_ContainerStateChanged() { }

        //Job-Hook
        public void Notify_FinishEmptyingToTarget()
        {
            if (Container.Empty)
            {
                DeSpawn();
                return;
            }
            currentDesignatedTarget = LocalTargetInfo.Invalid;
        }

        //
        public override void DrawGUIOverlay()
        {
            base.DrawGUIOverlay();
            if (Find.CameraDriver.CurrentZoom == CameraZoomRange.Closest)
            {
                Vector3 v = GenMapUI.LabelDrawPosFor(Position);
                GenMapUI.DrawThingLabel(v, Container.StoredPercent.ToStringPercent(), Color.white);
            }
        }

        public override void Draw()
        {
            base.Draw();
            if (HasValidTarget && Find.Selector.IsSelected(this))
            {
                GenDraw.DrawLineBetween(DrawPos, currentDesignatedTarget.CenterVector3, SimpleColor.White);
            }
        }

        //
        private TargetingParameters ForSelf => paramsInt ??= new TargetingParameters
        {
            canTargetBuildings = true,
            validator = (info) =>
            {
                if (info.Thing is ThingWithComps twc)
                {
                    var n = twc.TryGetComp<Comp_NetworkStructure>();
                    if (n == null) return false;
                    if (!n[NetworkDef].NetworkRole.HasFlag(NetworkRole.Storage)) return false;
                    if (!Container.AllStoredTypes.Any(n.AcceptsValue)) return false;
                    if (n[networkDef].Container.Full) return false;
                    return true;
                }
                return false;
            },
        };

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach (Gizmo g in base.GetGizmos())
            {
                yield return g;
            }
            foreach (Gizmo g in Container.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_Target
            {
                defaultLabel = "TELE.PortableContainer.Designator".Translate(),
                defaultDesc = "TELE.PortableContainer.DesignatorDesc".Translate(),
                icon = BaseContent.BadTex,
                targetingParams = ForSelf,
                action = (info) =>
                {
                    currentDesignatedTarget = info;
                }
            };
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            return sb.ToString().TrimEndNewlines();
        }
    }
}
