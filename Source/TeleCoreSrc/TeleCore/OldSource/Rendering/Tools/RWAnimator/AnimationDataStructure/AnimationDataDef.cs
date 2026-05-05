using System.Collections.Generic;
using System.Linq;
using System.Xml;
using Verse;

namespace TeleCore.Rendering.Tools.RWAnimator.AnimationDataStructure;

public class AnimationDataDef : Def, IExposable
{
    public List<AnimationSet> animationSets;

    public void ExposeData()
    {
        Scribe_Values.Look(ref defName, nameof(defName));
        Scribe_Collections.Look(ref animationSets, nameof(animationSets), LookMode.Deep);
    }

    public void Readout()
    {
        TLog.Message($"PostLoad Checkup: {animationSets != null} : {animationSets?.Count}");
        if (animationSets == null) return;
        for (var i = 0; i < animationSets.Count; i++)
        {
            var animationSet = animationSets[i];
            TLog.Message(
                $"Set[{i}]: {animationSet.HasTextures} [{animationSet.textureParts?.Count}]| {animationSet.HasAnimations} [{animationSet.animations?.Count}]");
            if (animationSet.textureParts != null)
                for (var t = 0; t < animationSet.textureParts.Count; t++)
                {
                    var texturePart = animationSet.textureParts[t];
                    TLog.Message(
                        $"TexturePart[{t}]: {texturePart.TexCoordAnchor} | {texturePart.AttachScript} | {texturePart.LayerIndex} | {texturePart.LayerTag} | {texturePart.Material?.name}");
                }

            if (animationSet.animations != null)
                for (var t = 0; t < animationSet.animations.Count; t++)
                {
                    var animationPart = animationSet.animations[t];
                    TLog.Message(
                        $"AnimationPart[{t}]: {animationPart.tag} | {animationPart.keyFrames.Sum(s => s.Count)}");
                }
        }
    }

    public override void PostLoad()
    {
        base.PostLoad();
    }

    //Loading
    private void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        defName = DirectXmlToObject.ObjectFromXml<string>(xmlRoot.SelectSingleNode(nameof(defName)), false);
        animationSets =
            DirectXmlToObject.ObjectFromXml<List<AnimationSet>>(xmlRoot.SelectSingleNode(nameof(animationSets)), true);
    }
}