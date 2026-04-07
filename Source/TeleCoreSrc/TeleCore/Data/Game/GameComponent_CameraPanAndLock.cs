using UnityEngine;
using Verse;

namespace TeleCore;

public class GameComponent_CameraPanAndLock : GameComponent
{
    //private bool panningInt = false;
    //private bool lockInt = false;
    private bool panning = false;
    private int currentTick = 0;
    private int durationTicks = -1;
    private float sizeInt = 24;

    private Vector3 StartingPos;
    private Vector3 EndingPos;

    public CameraDriver CameraDriver => Find.CameraDriver;

    private bool ReachedDestination => currentTick >= durationTicks;

    public GameComponent_CameraPanAndLock(Game game)
    {
    }

    public override void GameComponentTick()
    {
        base.GameComponentTick();
        if (durationTicks <= 0) return;
        var rootPos = Vector3.Lerp(StartingPos, EndingPos, currentTick / (float)durationTicks);
        CameraDriver.SetRootPosAndSize(rootPos, sizeInt);

        currentTick++;
        //Target Reached
        if (ReachedDestination)
        {
            Reset();
        }
    }

    private void SetData(IntVec3 from, IntVec3 to, float duration, float size = 24)
    {
        if (panning)
        {
            Log.Warning("Already panning, stop panning first before setting new pan data");
            return;
        }

        StartingPos = from.ToVector3Shifted();
        EndingPos = to.ToVector3Shifted();
        durationTicks = duration.SecondsToTicks();
        sizeInt = size;
    }

    private void Reset()
    {
        sizeInt = 24;
        durationTicks = -1;
        currentTick = 0;
        panning = false;
    }

    public void Stop()
    {
        Reset();
    }

    public void PanDirect(IntVec3 to, float duration, float size = 24)
    {
        StartPan(Find.CameraDriver.MapPosition, to, duration, size);
    }

    public void StartPan(IntVec3 from, IntVec3 to, float duration, float size)
    {
        SetData(from, to, duration, size);
    }

    public void Lock(IntVec3 pos, float duration, float size = 24)
    {
        SetData(pos, pos, duration, size);
    }
}