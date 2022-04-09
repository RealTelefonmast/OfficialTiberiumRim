using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class AnimationSaveLoader : UIElement
    {
        private TextureCanvas canvas;
        private AnimationData animationData;

        private Vector2 scrollPos = Vector2.zero;

        public override UIElementMode UIMode => UIElementMode.Static;

        public AnimationSaveLoader(TextureCanvas canvas)
        {
            this.canvas = canvas;
        }

        private void ConstructSaveData()
        {
            animationData = new AnimationData();
            animationData.allParts = canvas.ElementList.Select(t => (t as TextureElement).GetData()).ToList();
            label = Scribe.saver.DebugOutputFor(animationData);
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

            var bottomRect = inRect.BottomPartPixels(inRect.height - buttonRect.height).ContractedBy(5);
            Widgets.DrawMenuSection(bottomRect);

            Widgets.TextAreaScrollable(bottomRect, label, ref scrollPos, true);
        }
    }
}
