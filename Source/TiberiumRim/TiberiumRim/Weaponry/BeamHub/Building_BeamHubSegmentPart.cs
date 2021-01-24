using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RimWorld;
using Verse;

namespace TiberiumRim
{
    public class Building_BeamHubSegmentPart : Building
    {
        private List<Building_BeamHub> connectingHubs = new List<Building_BeamHub>(4) { null, null, null, null };
        private TRThingDef parentHubDef;
        private bool active = false;

        public bool ShouldBeActive
        {
            get
            {
                if (JunctionHub != null)
                {
                    for (int i = 0; i < 4; i++)
                    {
                        if (JunctionHub.IsConnectedAndPoweredIn(i))
                            return true;
                    }
                }
                return NSPowered || EWPowered;
            }
        }

        private Building_BeamHub junction;
        private Building_BeamHub JunctionHub
        {
            get
            {
                return junction ??= (Building_BeamHub) Position.GetFirstThing(Map, parentHubDef);
            }
        }

        public bool NSPowered => HasNSConnection && connectingHubs[0].IsConnectedAndPoweredIn(2);
        public bool EWPowered => HasEWConnection && connectingHubs[1].IsConnectedAndPoweredIn(3);

        public bool HasNSConnection => connectingHubs[0] != null && connectingHubs[2] != null; //.IsConnectedTo(connectingHubs[2], 2);
        public bool HasEWConnection => connectingHubs[1] != null && connectingHubs[3] != null;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref parentHubDef, "parentDef");
            Scribe_Collections.Look(ref connectingHubs, "connectingHubs", LookMode.Reference);
            Scribe_Values.Look(ref active, "active");
        }


        private int Opposite(int i)
        {
            switch (i)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 0;
                case 3:
                    return 1;
            }
            return 0;
        }

        public void RegisterConnection(Building_BeamHub startHub, Building_BeamHub endHub, int directionFrom)
        {
            parentHubDef ??= startHub.def;

            //Set connecting hubs
            connectingHubs[directionFrom] = startHub;
            connectingHubs[Opposite(directionFrom)] ??= endHub;
        }

        public void DeregisterConnection(int directionFrom)
        {
            //Reset connecting hubs
            connectingHubs[directionFrom] = null;
            connectingHubs[Opposite(directionFrom)] = null;

            if (Spawned)
            {
                CheckForUpdate();
                if (HasNSConnection || HasEWConnection) return;
                Position.GetFirstThing(Map, parentHubDef.beamHub.beamDef)?.DeSpawn();
                DeSpawn();
            }
        }

        private void CheckForUpdate()
        {
            JunctionHub?.UpdateSegmentToExistingConnections(this);
        }

        public void CheckBeamStatus()
        {
            if (ShouldBeActive && !active)
            {
                active = true;
                GenSpawn.Spawn(parentHubDef.beamHub.beamDef, Position, Map);
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Buildings);
            }

            if (!ShouldBeActive && active)
            {
                active = false;
                Position.GetFirstThing(Map, parentHubDef.beamHub.beamDef)?.DeSpawn();
                Map.mapDrawer.MapMeshDirty(Position, MapMeshFlag.Buildings);
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("\nShouldBeActive: " + ShouldBeActive);
            sb.AppendLine("NS Connection: " + NSPowered);
            sb.AppendLine("EW Connection: " + EWPowered);
            sb.AppendLine("Connecting Hubs:\n" + (connectingHubs[0] != null) + "\n"
                          + (connectingHubs[1] != null) + "\n"
                          + (connectingHubs[2] != null) + "\n"
                          + (connectingHubs[3] != null) + "\n");
            return sb.ToString().TrimEndNewlines();
        }
    }

    /*
    public class Building_BeamHubSegmentPart2 : Building
    {
        private List<Building_BeamHub> connectingHubs = new List<Building_BeamHub>(4) {null, null, null, null};
        private TRThingDef parentHubDef;
        private bool active = false;

        public bool AnyParentActive => connectingHubs.Any(hub => hub != null && hub.ShouldConnectWithOpposite(connectingHubs.IndexOf(hub))); //startHub || endHub  parentSegments.Any(s => s != null && s.IsActive);
        public bool AnyParentValid => connectingHubs.Any(hub => hub != null && hub.ConnectsWithAnyOpposite());
        
        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Defs.Look(ref parentHubDef, "parentDef");
            Scribe_Collections.Look(ref connectingHubs, "connectingHubs", LookMode.Reference);
            Scribe_Values.Look(ref active, "active");
        }

        private int Opposite(int i)
        {
            switch (i)
            {
                case 0:
                    return 2;
                case 1:
                    return 3;
                case 2:
                    return 0;
                case 3:
                    return 1;
            }
            return 0;
        }

        public void RegisterHub(Building_BeamHub hub, int directionTo)
        {
            parentHubDef ??= hub.def;
            connectingHubs[Opposite(directionTo)] ??= hub;
        }

        public void DeRegisterHub(Building_BeamHub hub, int direction)
        {
            if (UpdateIfNecessary(hub)) return;
            connectingHubs[Opposite(direction)] = null;
            if (!AnyParentValid && Spawned)
            {
                Notify_Deactivate(true);
                DeSpawn();
            }
        }

        private bool UpdateIfNecessary(Building_BeamHub hub)
        {
            foreach (var beamHub in hub.ConnectedHubs)
            {
                Log.Message("Connected to " + beamHub);
            }
            if (hub.Position == Position && hub.ConnectsWithAnyOpposite()) return true;
            return false;
        }

        public void Toggle()
        {
            if (active)
            {
                Notify_Deactivate();
            }
            else
            {
                Notify_Activate();
            }
        }

        private void Notify_Activate()
        {
            if (!active && AnyParentActive && Spawned)
            {
                active = true;
                GenSpawn.Spawn(parentHubDef.beamHub.beamDef, Position, Map);
            }
        }

        private void Notify_Deactivate(bool force = false)
        {
            if ((active && force) || (active && !AnyParentActive && Spawned))
            {
                active = false;
                Position.GetFirstThing(Map, parentHubDef.beamHub.beamDef)?.DeSpawn();
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("AnyParentActive: " + AnyParentActive);
            sb.AppendLine("Connecting Hubs:\n" + (connectingHubs[0] != null) + "\n"
                          + (connectingHubs[1] != null) + "\n"
                          + (connectingHubs[2] != null) + "\n"
                          + (connectingHubs[3] != null) + "\n");
            return sb.ToString().TrimEndNewlines();
        }
    }
    */
}
