using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class DevToolDef : Def
    {
        private Window windowInt;
        public Type windowClass;

        public Window GetWindow => windowInt ??= (Window)Activator.CreateInstance(windowClass);
    }

    public class Dialog_ToolSelection : Window
    {
        private List<DevToolDef> allDevTools;

        public override Vector2 InitialSize => new Vector2(900f, 700f);

        public Dialog_ToolSelection()
        { 
            forcePause = true;
            doCloseX = true;
            doCloseButton = true;
            closeOnClickedOutside = true;
            absorbInputAroundWindow = true;
        }

        public override void PreOpen()
        {
            base.PreOpen();
            TLog.Message($"DevTools: {DefDatabase<DevToolDef>.DefCount}");
            allDevTools ??= DefDatabase<DevToolDef>.AllDefsListForReading;
        }

        public override void DoWindowContents(Rect inRect)
        {
            Rect titleRect = inRect.TopPart(0.05f);
            Rect selectionRect = inRect.BottomPart(.95f);
            Text.Font = GameFont.Medium;
            Widgets.Label(titleRect, "Select a Tool");
            Text.Font = default;

            Widgets.BeginGroup(selectionRect);
            List<ListableOption> list = new List<ListableOption>();
            foreach (var devTool in allDevTools)
            {
                list.Add(new ListableOption(devTool.LabelCap, () => { Find.WindowStack.Add(devTool.GetWindow); }));
            }
            OptionListingUtility.DrawOptionListing(new Rect(0, 0, 200, selectionRect.height), list);
            Widgets.EndGroup();
        }
    }
}
