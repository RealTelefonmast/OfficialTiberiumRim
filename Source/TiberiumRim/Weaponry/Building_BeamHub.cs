namespace TiberiumRim
{
    /*
    public class Building_BeamHub2 : TRBuilding
    {
        private List<BeamSegment> Connections = new List<BeamSegment>(4);
        private bool[] allowedDirections = new bool[4] { true, true, true, true };
        private bool[] connections = new bool[4] { false, false, false, false };

        private Graphic toggleGraphic;
        public Graphic ToggleGraphic => toggleGraphic ??= props.beamHub.toggleGraphic.Graphic;

        public bool IsPowered => this.GetComp<CompPowerTrader>().PowerOn;

        public override void ExposeData()
        {
            base.ExposeData();
            Scribe_Collections.Look(ref Connections, "segments", LookMode.Deep);
            Scribe_Values.Look(ref allowedDirections, "directions");
            Scribe_Values.Look(ref connections, "connections");
        }

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
            TryConnectToHubs();
        }

        public override void DeSpawn(DestroyMode Role = DestroyMode.Vanish)
        {
            base.DeSpawn(Role);
            foreach (BeamSegment segment in Connections)
            {
                segment?.Destroy();
            }
        }

        private void TryConnectToHubs()
        {
            for (int i = 0; i < 4; i++)
            {
                if (!allowedDirections[i]) continue;
                LookForHub(new Rot4(i));
            }
        }

        private void Toggle(Rot4 rot, bool toggle)
        {
            if(!toggle || allowedDirections[rot.AsInt])
                Connections[rot.AsInt]?.Toggle(toggle);
        }

        private void LookForHub(Rot4 rot)
        {
            List<IntVec3> vecs = new List<IntVec3>();
            for (int i = 0; i <= props.beamHub.range; i++)
            {
                IntVec3 c = Position + new IntVec3(0, 0, i).RotatedBy(rot);
                if (!c.InBounds(Map)) break;
                vecs.Add(c);
                if(c.GetFirstBuilding(Map) is Building_BeamHub hub && hub.props == props && hub != this)
                {
                    BeamSegment segment = new BeamSegment(new Building_BeamHub[] { this, hub }, vecs);
                    SetConnection(segment, rot);
                    hub.SetConnection(segment, rot.Opposite);
                    break;
                }
            }
        }

        public void SetConnection(BeamSegment segment, Rot4 rot)
        {
            connections[rot.AsInt] = true;
            Connections[rot.AsInt] = segment;
        }

        public void RemoveConnection(BeamSegment segment)
        {
            int i = Connections.FirstIndexOf(x => x != null && x == segment);
            connections[i] = false;
            Connections[i] = null;
        }

        public override void Draw()
        {
            base.Draw();
            if (!Find.Selector.IsSelected(this)) return;
            foreach (BeamSegment segment in Connections)
            {
                if (segment != null)
                    GenDraw.DrawFieldEdges(segment.Cells, Color.cyan);
            }
        }

        protected override void ReceiveCompSignal(string signal)
        {
            base.ReceiveCompSignal(signal);
            if(signal == "PowerTurnedOn")
            {
                for (int i = 0; i < Connections.Count; i++)
                {
                    Toggle(new Rot4(i), true);
                }
            }
            else if (signal == "PowerTurnedOff")
            {
                for (int i = 0; i < Connections.Count; i++)
                {
                    Toggle(new Rot4(i), false);
                }
            }
        }

        public override string GetInspectString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat(base.GetInspectString());
            sb.AppendLine("\nConnections: " + Connections.Count());
            sb.AppendLine("Allowed Dirs:\n" + new Rot4(0).ToStringHuman() + " " + allowedDirections[0] + "\n"
                                            + new Rot4(1).ToStringHuman() + " " + allowedDirections[1] + "\n"
                                            + new Rot4(2).ToStringHuman() + " " + allowedDirections[2] + "\n"
                                            + new Rot4(3).ToStringHuman() + " " + allowedDirections[3] + "\n");
            return sb.ToString().TrimEndNewlines();
        }

        public override IEnumerable<Gizmo> GetGizmos()
        {
            foreach(Gizmo g in base.GetGizmos())
            {
                yield return g;
            }

            yield return new Command_Action
            {
                defaultLabel = "Toggle North",
                icon = (Texture2D)ToggleGraphic.MatNorth.mainTexture,
                action = delegate
                {
                    allowedDirections[0] = !allowedDirections[0];
                    Connections[0].OppositeHubFor(this).allowedDirections[2] = allowedDirections[0];
                    Log.Message("Toggling North to: " + allowedDirections[0]);
                    Toggle(Rot4.North, allowedDirections[0]);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Toggle South",
                icon = (Texture2D)ToggleGraphic.MatSouth.mainTexture,
                action = delegate
                {
                    allowedDirections[2] = !allowedDirections[2];
                    Connections[2].OppositeHubFor(this).allowedDirections[0] = allowedDirections[2];
                    Log.Message("Toggling South to: " + allowedDirections[2]);
                    Toggle(Rot4.South, allowedDirections[2]);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Toggle East",
                icon = (Texture2D)ToggleGraphic.MatEast.mainTexture,
                action = delegate
                {
                    allowedDirections[1] = !allowedDirections[1];
                    Connections[1].OppositeHubFor(this).allowedDirections[3] = allowedDirections[1];
                    Log.Message("Toggling East to: " + allowedDirections[1]);
                    Toggle(Rot4.East, allowedDirections[1]);
                }
            };

            yield return new Command_Action
            {
                defaultLabel = "Toggle West",
                icon = (Texture2D)ToggleGraphic.MatWest.mainTexture,
                action = delegate
                {
                    allowedDirections[3] = !allowedDirections[3];
                    Connections[3].OppositeHubFor(this).allowedDirections[1] = allowedDirections[3];
                    Log.Message("Toggling West to: " + allowedDirections[3]);
                    Toggle(Rot4.West, allowedDirections[3]);
                }
            };
        }
    }
    */
}
