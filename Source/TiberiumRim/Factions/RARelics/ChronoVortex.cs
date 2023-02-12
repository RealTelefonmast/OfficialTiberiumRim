using TeleCore;
using UnityEngine;
using Verse;
using Verse.Sound;

namespace TiberiumRim
{
    public class ChronoVortex : PortalSpawner
    {
        protected override int ParticleTick => 55;

        public override void SpawnSetup(Map map, bool respawningAfterLoad)
        {
            base.SpawnSetup(map, respawningAfterLoad);
        }

        protected override void DoParticleEffect()
        {
            IntVec3 randomCell1 = Position + GenRadial.RadialPattern[TRandom.Range(0, GenRadial.NumCellsInRadius(2.75f))];

            var from = Position;
            var to   = randomCell1;

            var normed = (to - from).ToVector3().normalized;
            IntVec3 toCell = from + (normed * Rand.Range(6, 8)).ToIntVec3();

            Mote_Arc arc = (Mote_Arc)ThingMaker.MakeThing(EffectsDefOf.Mote_Arc);
            Material mat = MaterialsTesla.Arcs[TRandom.Range(0, 3)];
            arc.fadeInTimeOverride = 0.25f;
            arc.solidTimeOverride = 0.25f;
            arc.fadeOutTimeOverride = 0.85f;
            arc.SetConnections(toCell.ToVector3Shifted(), randomCell1.ToVector3Shifted(), mat, Color.white);
            arc.Attach(null);
            GenSpawn.Spawn(arc, Position, Map, WipeMode.Vanish);
            SoundDef.Named("ElectricArc").PlayOneShot(SoundInfo.InMap(this, MaintenanceType.None));
        }
    }
}
