using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator.AnimationDataStructure;

public struct AnimationActionEventFlag
{
    public string eventTag;
    public int frameTick;

    public string EventTag => eventTag;
    public int Frame => frameTick;

    public AnimationActionEventFlag(int currentFrame, string flag)
    {
        frameTick = currentFrame;
        eventTag = flag;
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref frameTick, nameof(frameTick), 0, true);
        Scribe_Deep.Look(ref eventTag, nameof(eventTag));
    }
}