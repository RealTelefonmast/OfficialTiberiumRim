using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

internal class AnimationMetaData : IExposable
{
    //KeyFrame String Buffer
    internal const int BufferSize = 2 + 2 + 2 + 4 + 1;

    private readonly List<AnimationPartValue>[] animationsByRotation;

    //Animation
    internal string defName;
    private readonly List<UIElement>[] elementsByRotation;

    private AnimationDataDef internalDef;
    private Rot4 internalRot = Rot4.North;
    private List<int> lastSelectedAnimationIndex;
    private List<int> lastSelectedElementIndex;

    //Reference Data
    private readonly TextureCanvas parentCanvas;
    internal Dictionary<KeyFrame, string[]> StringBuffers = new();

    public AnimationMetaData(TextureCanvas parentCanvas)
    {
        this.parentCanvas = parentCanvas;
        lastSelectedElementIndex = new List<int> {-1, -1, -1, -1};
        lastSelectedAnimationIndex = new List<int> {-1, -1, -1, -1};
        //
        elementsByRotation = new List<UIElement>[4]
        {
            new(), new(), new(), new()
        };

        animationsByRotation = new List<AnimationPartValue>[4]
        {
            new(), new(), new(), new()
        };
    }

    public Rot4 CurRot => internalRot;
    public bool Initialized { get; private set; }

    public bool Loading { get; private set; }

    public int ElementIndex => lastSelectedElementIndex[CurRot.AsInt];
    public int AnimationIndex => lastSelectedAnimationIndex[CurRot.AsInt];
    public List<UIElement> CurrentElementList => elementsByRotation[CurRot.AsInt];
    public List<AnimationPartValue> CurrentAnimations => animationsByRotation[CurRot.AsInt];
    public AnimationPartValue SelectedAnimationPart { get; private set; }

    public void Notify_Init()
    {
        Initialized = true;
    }

    public AnimationDataDef ConstructAnimationDef()
    {
        var newDef = new AnimationDataDef();
        newDef.defName = defName;
        newDef.animationSets = new List<AnimationSet>(4);
        for (var i = 0; i < 4; i++) newDef.animationSets.Add(ConstructSetForRotation(i));
        return newDef;
    }

    private AnimationSet ConstructSetForRotation(int rot)
    {
        var animationSet = new AnimationSet();
        if (!elementsByRotation[rot].NullOrEmpty())
        {
            var orderedElements = elementsByRotation[rot].Select(t => t as TextureElement);
            animationSet.textureParts = orderedElements.Select(t => t.GetData()).ToList();
        }

        if (!animationsByRotation[rot].NullOrEmpty())
        {
            animationSet.animations = new List<Verse.AnimationPart>(animationsByRotation[rot].Count);
            foreach (var partValue in animationsByRotation[rot])
                animationSet.animations.Add(partValue.ToAnimationPart(animationSet.textureParts));
        }

        return animationSet;
    }

    private void LoadFromDef(AnimationDataDef dataDef)
    {
        defName = dataDef.defName;
        for (var i = 0; i < 4; i++) LoadSetFrom(dataDef, i);
    }

    private void LoadSetFrom(AnimationDataDef dataDef, int rot)
    {
        var animationSet = dataDef.animationSets[rot];

        if (animationSet.HasTextures)
            foreach (var textureData in animationSet.textureParts)
            {
                var newElement = new TextureElement(new Rect(Vector2.zero, parentCanvas.Size), textureData);
                parentCanvas.Notify_LoadedElement(newElement);
                elementsByRotation[rot].Add(newElement);
            }

        if (animationSet.HasAnimations)
            foreach (var animation in animationSet.animations)
                animationsByRotation[rot].Add(new AnimationPartValue(elementsByRotation[rot], animation));
    }

    public string[] BufferFor(KeyFrame frame)
    {
        if (StringBuffers.ContainsKey(frame)) return StringBuffers[frame];
        var buffer = new string[BufferSize];
        frame.Data.UpdateBuffer(buffer);
        StringBuffers.Add(frame, buffer);
        return StringBuffers[frame];
    }

    //
    public void Notify_RemoveAnimationPart(int index)
    {
        CurrentAnimations.RemoveAt(index);

        var nextIndex = CurrentAnimations.Count - 1;
        SetAnimationIndex(nextIndex);
        if (nextIndex >= 0)
            SelectedAnimationPart = CurrentAnimations[nextIndex];
        else
            SelectedAnimationPart = null;
    }

    public AnimationPartValue Notify_CreateNewAnimationPart(string tag, float lengthSeconds)
    {
        var animation = new AnimationPartValue(tag, lengthSeconds);
        CurrentAnimations.Add(animation);
        return animation;
    }

    public void Notify_PostCreateAnimation()
    {
        SelectedAnimationPart = CurrentAnimations.Last();
        SetAnimationIndex(CurrentAnimations.Count - 1);
    }

    public void CreateOrSetRotationSet(Rot4 newRotation)
    {
        internalRot = newRotation;
        if (Enumerable.Any(CurrentAnimations))
            SelectedAnimationPart = CurrentAnimations[AnimationIndex];
        else
            SelectedAnimationPart = null;
    }

    public void SetAnimationPart(AnimationPartValue animationPart)
    {
        SelectedAnimationPart = animationPart;
        SetAnimationIndex(CurrentAnimations.IndexOf(SelectedAnimationPart));
    }

    public void SetElementIndex(int index)
    {
        lastSelectedElementIndex[CurRot.AsInt] = index;
    }

    public void SetAnimationIndex(int index)
    {
        lastSelectedAnimationIndex[CurRot.AsInt] = index;
    }

    public bool AnimationPartsFor(int i)
    {
        return !animationsByRotation[i].NullOrEmpty();
    }

    #region SAVELOADING

    public void ExposeData()
    {
        if (Scribe.mode == LoadSaveMode.Saving) internalDef = ConstructAnimationDef();

        Scribe_Values.Look(ref defName, "defName");
        Scribe_Values.Look(ref internalRot, "internalRot");
        Scribe_Deep.Look(ref internalDef, "internalDef");

        Scribe_Collections.Look(ref lastSelectedElementIndex, "lastSelectedElementIndex");
        Scribe_Collections.Look(ref lastSelectedAnimationIndex, "lastSelectedAnimationIndex");
    }

    public void LoadAnimation()
    {
        Loading = true;

        Scribe_Deep.Look(ref internalDef, "internalDef");
        Scribe_Values.Look(ref internalRot, "internalRot");
        Scribe_Collections.Look(ref lastSelectedElementIndex, "lastSelectedElementIndex");
        Scribe_Collections.Look(ref lastSelectedAnimationIndex, "lastSelectedAnimationIndex");

        //GenDataFromDef
        LoadFromDef(internalDef);

        if (internalDef != null)
        {
            Notify_Init();
            SelectedAnimationPart = CurrentAnimations[AnimationIndex];
        }

        Loading = false;
    }

    #endregion
}

public class AnimationPartValue
{
    internal float _internalSeconds;
    internal string _internalSecondsBuffer;
    public int frames;

    public string tag;

    public AnimationPartValue(string tag, float seconds)
    {
        _internalSeconds = seconds;
        this.tag = tag;
        frames = seconds.SecondsToTicks();
        ReplayBounds = new IntRange(0, frames);

        InternalFrames = new Dictionary<IKeyFramedElement, Dictionary<int, KeyFrame>>();
        InternalEventFlags = new Dictionary<int, AnimationActionEventFlag>();
    }

    public AnimationPartValue(List<UIElement> uiElements, Verse.AnimationPart animationPart)
    {
        tag = animationPart.tag;
        frames = animationPart.frames;
        _internalSeconds = frames.TicksToSeconds();
        _internalSecondsBuffer = _internalSeconds.ToString();

        ReplayBounds = animationPart.bounds;

        InternalFrames = new Dictionary<IKeyFramedElement, Dictionary<int, KeyFrame>>();

        var index = 0;
        foreach (var keyFrameCollection in animationPart.keyFrames)
        {
            var texture = (TextureElement) uiElements[index];
            InternalFrames.Add(texture, new Dictionary<int, KeyFrame>());
            if (!keyFrameCollection.NullOrEmpty())
                foreach (var frame in keyFrameCollection)
                    InternalFrames[texture].Add(frame.Frame, frame);
            index++;
        }
    }

    internal IntRange ReplayBounds { get; set; }

    internal Dictionary<IKeyFramedElement, Dictionary<int, KeyFrame>> InternalFrames { get; }
    internal Dictionary<int, AnimationActionEventFlag> InternalEventFlags { get; }

    internal bool InternalDifference => _internalSeconds.SecondsToTicks() != frames;

    public void SetFrames()
    {
        frames = _internalSeconds.SecondsToTicks();
        ReplayBounds = new IntRange(0, frames);
    }

    public List<ScribeList<KeyFrame>> KeyFramesList(List<TextureData> orderBy)
    {
        var copy = InternalFrames.Copy().Where(i => i.Value.Any());
        var orderDesc =
            copy.OrderByDescending(x => orderBy.Count - 1 - orderBy.IndexOf((x.Key as TextureElement).GetData()));
        var select = orderDesc.Select(parentDict =>
            new ScribeList<KeyFrame>(parentDict.Value.Select(keyFrameDict => keyFrameDict.Value).ToList(),
                LookMode.Deep));
        var toList = select.ToList();

        return toList;
        return InternalFrames.Copy()
            .OrderByDescending(x => orderBy.Count - 1 - orderBy.IndexOf((x.Key as TextureElement).GetData()))
            .Select(parentDict =>
                new ScribeList<KeyFrame>(parentDict.Value.Select(keyFrameDict => keyFrameDict.Value).ToList(),
                    LookMode.Deep))
            .ToList();
    }

    public Verse.AnimationPart ToAnimationPart(List<TextureData> orderBy)
    {
        return new Verse.AnimationPart
        {
            tag = tag,
            frames = frames,
            bounds = ReplayBounds,
            keyFrames = KeyFramesList(orderBy)
            //eventFlags = InternalEventFlags.OrderByDescending(t => frames- t.Key).Select(t => t.Value).ToList(),
        };
    }
}