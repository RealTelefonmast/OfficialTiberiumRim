using System.Collections.Generic;
using Verse;

namespace TeleCore.Unsorted;

public struct AnimationPart : IExposable
{
    public string tag;
    public int frames;

    /// <summary>
    ///     The list of keyframe collections for each material defined by the parent textureParts list
    /// </summary>
    /// ScribeList
    public List<ScribeList<KeyFrame>> keyFrames;

    public List<AnimationActionEventFlag> eventFlags;
    public IntRange bounds;

    //Loading
    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving)
            //Sort keyframes by their frame, as it is not handled in the editor
            foreach (var list in keyFrames)
                list.SortBy(k => k.Frame);

        Scribe_Values.Look(ref tag, nameof(tag));
        Scribe_Values.Look(ref frames, nameof(frames), 0, true);
        Scribe_Values.Look(ref bounds, nameof(bounds), new IntRange(0, frames), true);
        Scribe_Collections.Look(ref keyFrames, nameof(keyFrames), LookMode.Deep);
        Scribe_Collections.Look(ref eventFlags, nameof(eventFlags), LookMode.Deep);
    }
}