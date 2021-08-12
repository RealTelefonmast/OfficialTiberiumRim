using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public enum MoteThrowerType
    {
        TickBased,
        ChanceBased
    }

    public class MoteThrowerInfo
    {
        public ThingDef moteDef;
        public SoundDef soundDef;
        public MoteThrowerType type = MoteThrowerType.TickBased;
        public FloatRange speed = new FloatRange(0f, 0f);
        public FloatRange scale = FloatRange.One;
        public FloatRange rotation = new FloatRange(0f, 360f);
        public FloatRange rotationRate = new FloatRange(0f, 0f);
        public FloatRange angle = new FloatRange(0f, 360f);
        public FloatRange airTime = new FloatRange(999999f, 999999f);
        public IntRange burstCount = new IntRange(1, 1);
        public IntRange burstRange = new IntRange(100, 100);
        //public Color color = Color.white;
        public Vector3 positionOffset = Vector3.zero;
        public FloatRange solidTime = FloatRange.Zero;
        public float positionRadius = 0;
        public bool affectedByWind = false;

        public IntRange burstInterval = new IntRange(0, 0);
        public IntRange moteInterval = new IntRange(40, 100);
        public IntRange soundInterval = new IntRange(40, 100);
        public float chancePerTick = 0.1f;


    }

    public class MoteThrower
    {
        public Thing parent;
        public MoteThrowerInfo Info;
        private Room cachedRoom;
        private int ticksLeft = 0;
        private int ticksUntilBurst = 0;
        private int burstLeft = 0;

        public MoteThrower(MoteThrowerInfo info, Thing parent)
        {
            this.parent = parent;
            Info = info;
        }

        public void ThrowerTick(Vector3 pos, Map map)
        {          
            switch (Info.type)
            {
                case MoteThrowerType.TickBased:
                    if (Info.burstInterval.Average > 0)
                        if (ticksUntilBurst > 0)
                            ticksUntilBurst--;
                        else if (burstLeft > 0)
                        {
                            burstLeft--;
                            MakeMote(pos, map);
                        }
                        else
                            ResetBurst();
                    else
                    {
                        ticksLeft--;
                        if (ticksLeft <= 0)
                        {
                            MakeMote(pos, map);
                            ticksLeft = TRUtils.Range(Info.moteInterval);
                        }
                    }
                    return;
                case MoteThrowerType.ChanceBased:
                    if (TRUtils.Chance(Info.chancePerTick))
                    {
                        MakeMote(pos, map);
                    }
                    return;
            }
        }

        private void ResetBurst()
        {
            ticksUntilBurst = TRUtils.Range(Info.burstInterval);
            burstLeft = TRUtils.Range(Info.burstRange);
        }

        public void MakeMote(Vector3 exactPos, Map map)
        {
            IntVec3 spawnPos = exactPos.ToIntVec3();
            if (!spawnPos.InBounds(map)) return;
            int rand = TRUtils.Range(Info.burstCount);
            for (int i = 0; i < rand; i++)
            {
                Mote mote = (Mote)ThingMaker.MakeThing(Info.moteDef);
                mote.Scale = TRUtils.Range(Info.scale);
                mote.exactPosition = exactPos + Info.positionOffset + Gen.RandomHorizontalVector(Info.positionRadius);
                mote.exactRotation = TRUtils.Range(Info.rotation);
                mote.rotationRate = TRUtils.Range(Info.rotationRate);
                mote.solidTimeOverride = Info.solidTime.Average > 0 ? Info.solidTime.RandomInRange : -1f;
                if (mote is MoteThrown thrown)
                {
                    thrown.airTimeLeft = TRUtils.Range(Info.airTime);
                    float speed = TRUtils.Range(Info.speed);
                    float angle = TRUtils.Range(Info.angle);
                    if (Info.affectedByWind)
                    {
                        float windSpeed = Room.PsychologicallyOutdoors ? map.windManager.WindSpeed : 0f;
                        float windPct = Mathf.InverseLerp(0f, 2f, windSpeed);
                        speed *= Mathf.Lerp(0.1f, 1, windPct);
                        angle = (int)Mathf.Lerp(Info.angle.min, Info.angle.max, windPct);
                    }
                    thrown.SetVelocity(angle, speed);
                }
                GenSpawn.Spawn(mote, spawnPos, map);
            }
        }

        private Room Room
        {
            get
            {
                if(cachedRoom == null)
                {
                    cachedRoom = parent.GetRoomIndirect();
                }
                return cachedRoom;
            }
        }
    }
}
