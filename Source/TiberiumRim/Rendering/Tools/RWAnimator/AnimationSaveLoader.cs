using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AnimationSaveLoader : UIElement
    {
        private TextureCanvas canvas;
        private AnimationData animationData;

        private Vector2 scrollPos = Vector2.zero;

        public override string Label => "Save & Load";

        public AnimationSaveLoader(TextureCanvas canvas, Rect rect, UIElementMode mode) : base(rect, mode)
        {
            this.canvas = canvas;
        }

        private void ConstructSaveData()
        {
            animationData = new AnimationData();
            animationData.allParts = canvas.ElementList.Select(t => (t as TextureElement).GetData()).ToList();
            var p = animationData.allParts;
            //Orders the keyframe dictionary to correspond to the element layers, creating a list of keyframe lists, where the index of the first list corresponds to the index of the element
            animationData.keyFramesOrdered = canvas.TimeLine.framedElements.Copy()
                .OrderByDescending((x) => (p.Count-1) - p.IndexOf((x.Key as TextureElement).GetData()))
                .Select(parentDict => new ScribeList<KeyFrame>(parentDict.Value.Select(keyFrameDict => keyFrameDict.Value).ToList(), LookMode.Deep))
                .ToList();

            label = Scribe.saver.DebugOutputFor(animationData);
            //var reader = new XmlTextReader(new StreamReader(new MemoryStream(Encoding.UTF8.GetBytes(label))));
            //XmlDocument document = new XmlDocument();
            //document.Load(reader);
            
            WriteSettings();
        }

        private string label;
        private void WriteSettings()
        {
            Scribe.saver.InitSaving(Path.Combine("C:\\Users\\Maxim\\Desktop\\OutTest", "SavedAnimFile.xml"), "SavedAnim");
            try
            {
                Scribe_Deep.Look(ref animationData, "AnimationData");
            }
            finally
            {
                //Scribe.saver.writer.
                Scribe.saver.FinalizeSaving();
            }
        }

        protected override void DrawContents(Rect inRect)
        {
            base.DrawContents(inRect);
            var buttonRect = new Rect(inRect.x, inRect.y, 130, 30);
            if (Widgets.ButtonText(buttonRect, "Construct"))
            {
                ConstructSaveData();
            }

            var bottomRect = inRect.BottomPartPixels(inRect.height - buttonRect.height).ContractedBy(4);
            Widgets.DrawMenuSection(bottomRect);

            Widgets.TextAreaScrollable(bottomRect.ContractedBy(1), label, ref scrollPos, true);
        }
    }
}
