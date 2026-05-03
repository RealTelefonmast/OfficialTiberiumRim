using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

//TODO: Make new fleck based on thrown which has speed falloff and flight curves
public struct FleckExtended : IFleck
{
    public FleckDef def;
    public FleckDrawPosition position;
    public int setupTick;
    public Vector3 spawnPosition;

    public void Setup(FleckCreationData creationData)
    {
    }

    public bool TimeInterval(float deltaTime, Map map)
    {
        return true;
    }

    public void Draw(DrawBatch batch)
    {
    }

    public Vector3 GetPosition()
    {
        return spawnPosition;
    }
}