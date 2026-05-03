using System;
using System.Collections.Generic;
using TeleCore.Defs;
using TeleCore.Unsorted;
using UnityEngine;
using Verse;
using AnimationPart = Verse.AnimationPart;

namespace TeleCore.Comps;

public struct RuntimeAnimationPart
{
    public string tag;
    public Rot4 side;
    public int frames;
    public IntRange bounds;

    //
    public List<TextureData> textures;
    public List<KeyFrameChain> subChains; //Chain for each texture making this animation

    //
    public bool Invalid => tag == null;
    public static RuntimeAnimationPart Empty => new();

    public static RuntimeAnimationPart Create(AnimationSet set, AnimationPart part, Rot4 side)
    {
        var animation = new RuntimeAnimationPart
        {
            tag = part.tag,
            side = side,
            frames = part.frames,
            bounds = part.bounds,
            textures = set.textureParts,
            subChains = new List<KeyFrameChain>(set.textureParts.Count)
        };

        //
        animation.PopulateSubChains(part);
        return animation;
    }

    private void PopulateSubChains(AnimationPart part)
    {
        foreach (var partFrames in part.keyFrames)
            //Create chain for texture of animation
            subChains.Add(KeyFrameChain.Create(partFrames, part.bounds));
    }
}

public struct KeyFrameChain
{
    private int allocated;
    private RuntimeKeyFrame[] internalChain;

    public RuntimeKeyFrame this[int index]
    {
        get
        {
            if (index < 0)
                // Return the first keyframe in the chain
                return First;

            if (index >= internalChain.Length)
                // Return the last keyframe in the chain
                return Last;

            return internalChain[index];
        }
    }

    public RuntimeKeyFrame First => internalChain[0];
    public RuntimeKeyFrame Last => internalChain[internalChain.Length - 1];

    public static KeyFrameChain Empty = new();

    //
    public KeyFrameChain Allocate(int size)
    {
        internalChain = new RuntimeKeyFrame[size];
        return this;
    }

    public KeyFrameChain SetNext(KeyFrameData keyframe)
    {
        internalChain[allocated] = new RuntimeKeyFrame(this, keyframe, allocated);
        allocated++;
        return this;
    }

    //[#][#][F][I][I][I][F][I][I][I][I][F][#][#]
    public static KeyFrameChain Create(ScribeList<KeyFrame> frames, IntRange partBounds)
    {
        var newChain = new KeyFrameChain();
        newChain.Allocate(partBounds.max - partBounds.min);

        //Latest Fixed Frame
        var latestFixedInd = 0;
        var curFrame = 0;

        while (curFrame < newChain.internalChain.Length)
        {
            var curFixedFrame = frames[latestFixedInd];
            var lastFixedFrame = latestFixedInd - 1 < 0 ? curFixedFrame : frames[latestFixedInd - 1];
            //var nextFixedFrame = latestFixedInd + 1 > frames.Count - 1 ? curFixedFrame : frames[latestFixedInd + 1];
            //If the current frame is the same as the latest fixed frame, add it
            if (curFrame == curFixedFrame.Frame)
            {
                newChain.SetNext(curFixedFrame.Data);
                if (latestFixedInd < frames.Count - 1)
                    latestFixedInd++;
            }
            else //If at start or end, use the first or last frame
            if (curFixedFrame == lastFixedFrame)
            {
                newChain.SetNext(curFixedFrame.Data);
            }
            else //Otherwise, Interpolate
            {
                newChain.SetNext(lastFixedFrame.Data.Interpolated(curFixedFrame.Data,
                    Mathf.InverseLerp(lastFixedFrame.Frame, curFixedFrame.Frame, curFrame)));
            }

            curFrame++;
        }

        return newChain;
    }
}

public readonly struct RuntimeKeyFrame
{
    private readonly KeyFrameChain chain;

    public KeyFrameData KeyFrame { get; }
    public int Index { get; }

    public RuntimeKeyFrame Next => chain[Index + 1];
    public RuntimeKeyFrame Previous => chain[Index - 1];
    public static RuntimeKeyFrame Empty => new();

    public RuntimeKeyFrame(KeyFrameChain chain, KeyFrameData data, int index)
    {
        this.chain = chain;
        KeyFrame = data;
        Index = index;
    }
}

public class AnimationHolder
{
    public AnimationHolder(AnimationDataDef animationDef)
    {
        Animations = new Dictionary<(Rot4 rotation, string animTag), RuntimeAnimationPart>(4);
        //Go through each side of the thing
        for (var i = 0; i < 4; i++)
        {
            //Check if the side has an animationSet
            var setBySide = animationDef.animationSets[i];
            if (!(setBySide.HasAnimations && setBySide.HasTextures)) continue;

            /*
            texturesBySide[i] = new TextureData[set.textureParts.Count];
            //Go through each texture on the current animation set (each side has the same textures, for each animation)
            for (var k = 0; k < set.textureParts.Count; k++)
            {
                var textureData = set.textureParts[k];
                texturesBySide[i][k] = (textureData);
            }
            */

            //Go through each animation part of a set
            foreach (var animPart in setBySide.animations)
            {
                //Create RuntimeAnimationPart that generates all necessary keyframes
                var runtimePart = RuntimeAnimationPart.Create(setBySide, animPart, new Rot4(i));
                Animations.Add((new Rot4(i), animPart.tag), runtimePart);
            }
        }
    }


    internal Dictionary<(Rot4 rotation, string animTag), RuntimeAnimationPart> Animations { get; }

    public bool AnimationExists(string tag, Rot4 forSide, out RuntimeAnimationPart animation)
    {
        return Animations.TryGetValue((forSide, tag), out animation);
    }
}

public class AnimationProperties
{
    public AnimationDataDef animationDef;
    public string defaultAnimationTag;
}

public class RuntimeAnimationRenderer
{
    private readonly AnimationHolder _holder;
    private readonly AnimationProperties _props;
    private readonly Func<Rot4> _sideGetter;
    private int currentFrame;

    //
    //private RuntimeAnimationPart _curAnim;
    private string currentTag;
    private bool sustain;

    public RuntimeAnimationRenderer(AnimationProperties props, Func<Rot4> sideGetter = null)
    {
        _props = props;
        _holder = new AnimationHolder(props.animationDef);
        _sideGetter = sideGetter;
    }


    //Animation Relative to Rotation
    public Rot4 CurRotation => _sideGetter?.Invoke() ?? Rot4.North;

    public RuntimeAnimationPart CurAnimation => _holder.AnimationExists(currentTag, CurRotation, out var anim)
        ? anim
        : RuntimeAnimationPart.Empty;

    public void Start(string tag, bool sustain = false)
    {
        if (_holder.AnimationExists(tag, CurRotation, out _))
        {
            if (currentTag == tag && sustain) return;
            currentTag = tag;
            currentFrame = 0;
            this.sustain = sustain;
            return;
        }

        TLog.Error($"{_props.animationDef} does not contain animation tag '{tag}'");
    }

    public void Stop()
    {
        currentTag = null;
        currentFrame = 0;
        sustain = false;
        if (_props.defaultAnimationTag != null) Start(_props.defaultAnimationTag, true);
    }

    //
    public void TickRenderer()
    {
        //Skip when no animation selected
        if (currentTag == null) return;

        //
        currentFrame++;

        if (CurAnimation.bounds.max < currentFrame)
        {
            if (sustain)
            {
                currentFrame = 0;
                return;
            }

            Stop();
        }

        //
        currentFrame++;
    }

    //

    private RuntimeKeyFrame CurrentKeyFrameFor(RuntimeAnimationPart animation, int textureIndex)
    {
        if (currentTag == null) return RuntimeKeyFrame.Empty;
        return animation.subChains[textureIndex][currentFrame];
    }

    public void DrawAt(Vector3 drawPos, Vector2 drawSize, Rot4 side, bool shouldFlip = false)
    {
        if (currentTag == null) return;

        //Set initial layer pos
        var curAnim = CurAnimation;
        drawPos.y += Altitudes.AltInc * curAnim.textures.Count;

        //Iterate through animation layers, starting with first
        for (var i = 0; i < curAnim.textures.Count; i++)
        {
            //
            var texture = curAnim.textures[i];
            var keyFrame = CurrentKeyFrameFor(curAnim, i);
            var frame = keyFrame.KeyFrame;

            if (Equals(keyFrame, RuntimeKeyFrame.Empty)) _ = keyFrame.Index;

            var drawOffset = PixelToCellOffset(frame.TPosition, drawSize);
            var rotation = frame.TRotation;

            var size = frame.TSize * frame.TexCoords.size * drawSize;
            var actualSize = frame.TSize * texture.TexCoordReference.size * drawSize;

            if (shouldFlip)
            {
                drawOffset.x = -drawOffset.x;
                rotation = -rotation;
            }

            var newDrawPos = drawPos + drawOffset;
            newDrawPos += GenTransform.OffSetByCoordAnchor(actualSize, size, texture.TexCoordAnchor);
            if (frame.PivotPoint != Vector2.zero)
            {
                var pivotPoint = newDrawPos + PixelToCellOffset(frame.PivotPoint, drawSize);
                var relativePos = rotation.ToQuat() * (newDrawPos - pivotPoint);
                newDrawPos = pivotPoint + relativePos;
            }

            texture.Material.SetTextureOffset("_MainTex", frame.TexCoords.position);
            texture.Material.SetTextureScale("_MainTex", frame.TexCoords.size);

            var mesh = shouldFlip ? MeshPool.GridPlaneFlip(size) : MeshPool.GridPlane(size);
            Graphics.DrawMesh(mesh, newDrawPos, rotation.ToQuat(), texture.Material, 0, null, 0);

            //Go down to next layer
            drawPos.y -= Altitudes.AltInc;
        }
    }

    private static Vector3 PixelToCellOffset(Vector2 pixelOffset, Vector2 drawSize)
    {
        var width = pixelOffset.x / BaseCanvas._TileSize * drawSize.x;
        var height = -(pixelOffset.y / BaseCanvas._TileSize * drawSize.y);
        return new Vector3(width, 0, height);
    }

    public IEnumerable<Gizmo> GetGizmos()
    {
        yield return new Command_Action
        {
            defaultLabel = "Run Animation Once",
            action = delegate
            {
                var options = new List<FloatMenuOption>();
                foreach (var animationPart in _holder.Animations.Values)
                    options.Add(new FloatMenuOption($"Run {animationPart.side}_{animationPart.tag}",
                        delegate { Start(animationPart.tag); }));

                Find.WindowStack.Add(new FloatMenu(options));
            }
        };
    }
}

public class Comp_AnimationRenderer : ThingComp
{
    //
    private RuntimeAnimationRenderer renderer;

    public CompProperties_AnimationRenderer Props => (CompProperties_AnimationRenderer)props;

    //Current Animation Data
    private bool ShouldFlip => parent.Rotation == Rot4.West;

    private Rot4 UsedRotation => ShouldFlip ? Rot4.East : parent.Rotation;

    private Vector3 UsedDrawPos
    {
        get
        {
            if (parent is Pawn pawn) return pawn.drawer.DrawPos;

            return parent.DrawPos;
        }
    }

    private Vector2 UsedDrawSize
    {
        get
        {
            if (parent is Pawn pawn) return pawn.ageTracker.CurKindLifeStage.bodyGraphicData.drawSize;

            return parent.Graphic.drawSize;
        }
    }

    public override void PostSpawnSetup(bool respawningAfterLoad)
    {
        base.PostSpawnSetup(respawningAfterLoad);
        if (Props.animationDef == null) return;

        renderer = new RuntimeAnimationRenderer(new AnimationProperties
        {
            animationDef = Props.animationDef,
            defaultAnimationTag = Props.defaultAnimationTag
        }, () => UsedRotation);

        //Set internal ticker for animations
        GlobalUpdateEventHandler.GameTick += Ticker;

        //Start default animation
        if (Props.defaultAnimationTag != null) renderer.Start(Props.defaultAnimationTag, true);
    }

    public void Start(string tag, bool sustain = false)
    {
        renderer.Start(tag, sustain);
    }

    public void Stop()
    {
        renderer.Stop();
    }

    private void Ticker()
    {
        renderer.TickRenderer();
    }

    public override void PostDraw()
    {
        renderer.DrawAt(UsedDrawPos, UsedDrawSize, UsedRotation, ShouldFlip);
    }

    public override IEnumerable<Gizmo> CompGetGizmosExtra()
    {
        foreach (var gizmo in renderer.GetGizmos()) yield return gizmo;
    }
}

public class CompProperties_AnimationRenderer : Verse.CompProperties
{
    public AnimationDataDef animationDef;
    public string defaultAnimationTag;

    public CompProperties_AnimationRenderer()
    {
        compClass = typeof(Comp_AnimationRenderer);
    }
}