using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator.AnimationDataStructure;

public struct KeyFrame : IExposable
{
    private KeyFrameData frameData;
    private int frameTick;

    public KeyFrameData Data => frameData;
    public int Frame => frameTick;

    public KeyFrame(KeyFrameData data, int tick)
    {
        frameData = data;
        frameTick = tick;
    }

    public float Second => frameTick.TicksToSeconds();
    public bool IsValid => frameTick >= 0;
    public static KeyFrame Invalid => new(new KeyFrameData(), -1);

    public static bool operator ==(KeyFrame frame1, KeyFrame frame2)
    {
        if (frame1.frameTick != frame2.frameTick) return false;
        if (frame1.frameData != frame2.frameData) return false;
        return true;
    }

    public static bool operator !=(KeyFrame frame1, KeyFrame frame2)
    {
        if (frame1.frameTick != frame2.frameTick) return true;
        if (frame1.frameData != frame2.frameData) return true;
        return false;
    }

    //
    public bool Equals(KeyFrame other)
    {
        return frameData.Equals(other.frameData) && frameTick == other.frameTick;
    }

    public override bool Equals(object obj)
    {
        return obj is KeyFrame other && Equals(other);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            return (frameData.GetHashCode() * 397) ^ frameTick;
        }
    }

    //Loading
    public void ExposeData()
    {
        Scribe_Values.Look(ref frameTick, nameof(frameTick), 0, true);
        Scribe_Deep.Look(ref frameData, nameof(frameData));
    }
}