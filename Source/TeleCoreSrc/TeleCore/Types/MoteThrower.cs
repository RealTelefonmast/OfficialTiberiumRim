using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

public enum MoteThrowerType
{
    TickBased,
    ChanceBased
}

public class MoteThrowerInfo
{
    public bool affectedByWind = false;
    public FloatRange airTime = new(999999f, 999999f);
    public FloatRange angle = new(0f, 360f);
    public IntRange burstCount = new(1, 1);

    public IntRange burstInterval = new(0, 0);
    public IntRange burstRange = new(100, 100);
    public float chancePerTick = 0.1f;
    public ThingDef moteDef;

    public IntRange moteInterval = new(40, 100);

    //public Color color = Color.white;
    public Vector3 positionOffset = Vector3.zero;
    public float positionRadius = 0;
    public FloatRange rotation = new(0f, 360f);
    public FloatRange rotationRate = new(0f, 0f);
    public FloatRange scale = FloatRange.One;
    public FloatRange solidTime = FloatRange.Zero;
    public SoundDef soundDef;
    public IntRange soundInterval = new(40, 100);
    public FloatRange speed = new(0f, 0f);
    public MoteThrowerType type = MoteThrowerType.TickBased;
}

public class MoteThrower
{
    private int burstLeft;
    private Room cachedRoom;
    public MoteThrowerInfo Info;
    public Thing parent;
    private int ticksLeft;
    private int ticksUntilBurst;

    public MoteThrower(MoteThrowerInfo info, Thing parent)
    {
        this.parent = parent;
        Info = info;
    }

    private Room Room
    {
        get
        {
            if (cachedRoom == null) cachedRoom = parent.GetRoomIndirect();
            return cachedRoom;
        }
    }

    private Room Room
    {
        get
        {
            if (cachedRoom == null) cachedRoom = parent.GetRoomIndirect();
            return cachedRoom;
        }
    }

    public void ThrowerTick(Vector3 pos, Map map)
    {
        switch (Info.type)
        {
            case MoteThrowerType.TickBased:
                if (Info.burstInterval.Average > 0)
                {
                    if (ticksUntilBurst > 0)
                    {
                        ticksUntilBurst--;
                    }
                    else if (burstLeft > 0)
                    {
                        burstLeft--;
                        MakeMote(pos, map);
                    }
                    else
                    {
                        ResetBurst();
                    }
                }
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
                if (TRUtils.Chance(Info.chancePerTick)) MakeMote(pos, map);
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
        var spawnPos = exactPos.ToIntVec3();
        if (!spawnPos.InBounds(map)) return;
        var rand = TRUtils.Range(Info.burstCount);
        for (var i = 0; i < rand; i++)
        {
            var mote = (Mote)ThingMaker.MakeThing(Info.moteDef);
            mote.Scale = TRUtils.Range(Info.scale);
            mote.exactPosition = exactPos + Info.positionOffset + Gen.RandomHorizontalVector(Info.positionRadius);
            mote.exactRotation = TRUtils.Range(Info.rotation);
            mote.rotationRate = TRUtils.Range(Info.rotationRate);
            mote.solidTimeOverride = Info.solidTime.Average > 0 ? Info.solidTime.RandomInRange : -1f;
            if (mote is MoteThrown thrown)
            {
                thrown.airTimeLeft = TRUtils.Range(Info.airTime);
                var speed = TRUtils.Range(Info.speed);
                var angle = TRUtils.Range(Info.angle);
                if (Info.affectedByWind)
                {
                    var windSpeed = Verse.Room.PsychologicallyOutdoors ? map.windManager.WindSpeed : 0f;
                    var windPct = Mathf.InverseLerp(0f, 2f, windSpeed);
                    speed *= Mathf.Lerp(0.1f, 1, windPct);
                    angle = (int)Mathf.Lerp(Info.angle.min, Info.angle.max, windPct);
                }

                thrown.SetVelocity(angle, speed);
            }

            GenSpawn.Spawn(mote, spawnPos, map);
        }
    }
}