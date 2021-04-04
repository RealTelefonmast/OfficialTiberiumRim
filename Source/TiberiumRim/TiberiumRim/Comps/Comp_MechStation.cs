using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Verse;
using Verse.AI;

namespace TiberiumRim
{
    public class Comp_MechStation : Comp_Upgradable, IMechGarage<MechanicalPawn>
    {
        private MechGarage garage;
        private MechLink link;

        //List of mechs made/connected by this station, can be spawned or stored in a garage
        public MechLink MainMechLink => link ??= new MechLink(Props.mechCapacity);
        public MechGarage MainGarage => garage ??= new MechGarage(Props.garageCapacity);

        public List<MechanicalPawn> ConnectedMechs => MainMechLink.LinkedMechs;
        
        public CompProperties_MechStation Props => (CompProperties_MechStation)base.props;

        public override void PostExposeData()
        {
            base.PostExposeData();
            Scribe_Deep.Look(ref link, "mechHolder", Props.mechCapacity);
            Scribe_Deep.Look(ref garage, "mechGarage", Props.garageCapacity);
        }

        public override void PostSpawnSetup(bool respawningAfterLoad)
        {
            base.PostSpawnSetup(respawningAfterLoad);
        }

        public override void PostDestroy(DestroyMode mode, Map previousMap)
        {
            base.PostDestroy(mode, previousMap);
        }

        public void SendToGarage(MechanicalPawn mech)
        {
            if (!MainMechLink.Contains(mech))
            {
                //Transfer foreign mech to local link
                mech.ParentMechLink ??= MainMechLink;
                if (mech.ParentMechLink != MainMechLink)
                {
                    if (!mech.ParentMechLink.TryTransferTo(MainMechLink, mech)) 
                        return;
                }
            }
            if (garage.TryPushToGarage(mech))
            {
                //
            }
        }

        public MechanicalPawn ReleaseFromGarageDirect(MechanicalPawn mech)
        {
            if (garage.TryPullFromGarage(mech, out Thing result, parent.InteractionCell, parent.Map))
            {
                return (MechanicalPawn)result;
            }
            return null;
        }

        public MechanicalPawn ReleaseFromGarage(MechanicalPawn mech, Map map, IntVec3 pos, ThingPlaceMode placeMode = ThingPlaceMode.Direct)
        {
            if (garage.TryPullFromGarage(mech, out Thing result, pos, map, placeMode))
            {
                //
                return (MechanicalPawn)result;
            }
            return null;
        }

        public bool TryAddMech(MechanicalPawn mech, bool pushToGarage = false)
        {
            if (MainMechLink.TryConnectNewMech(mech))
            {
                return !pushToGarage || MainGarage.TryPushToGarage(mech);
            }
            return false;
        }

        public void RemoveMech(MechanicalPawn mech, bool pullFromGarage = false)
        {
            MainMechLink.RemoveMech(mech);
        }

        public MechanicalPawn MakeMech(MechanicalPawnKindDef kindDef)
        {
            if (!MainMechLink.CanHaveNewMech) return null;
            MechanicalPawn mech = (MechanicalPawn)PawnGenerator.GeneratePawn(kindDef, parent.Faction);
            mech.ageTracker.AgeBiologicalTicks = 0;
            mech.ageTracker.AgeChronologicalTicks = 0;
            mech.Rotation = Rot4.Random;
            mech.ParentBuilding = this.parent as Building;
            mech.Drawer.renderer.graphics.ResolveAllGraphics();
            mech.ParentMechLink = this.MainMechLink;
            return mech;
        }

        public MechanicalPawn SpawnMech(MechanicalPawn mech)
        {
            return (MechanicalPawn) GenSpawn.Spawn(mech, parent.InteractionCell, parent.Map);
        }

    }

    public class CompProperties_MechStation : CompProperties_Upgrade
    {
        public List<MechRecipeDef> mechRecipes = new List<MechRecipeDef>();
        public MechanicalPawnKindDef mechKindDef;
        public int mechCapacity = -1;
        public int garageCapacity = 1;

        public CompProperties_MechStation()
        {
            this.compClass = typeof(Comp_MechStation);
        }
    }
}
