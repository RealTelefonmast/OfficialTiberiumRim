using System;
using UnityEngine;
using Verse;

namespace TiberiumRim
{
    public class Gizmo_TiberiumStorage : Gizmo
    {
        public TiberiumContainer container;
        private static bool optionToggled = false;

        public Gizmo_TiberiumStorage()
        {
            this.order = -200f;
        }

        // Token: 0x0600298E RID: 10638 RVA: 0x0013B06B File Offset: 0x0013946B
        public override float GetWidth(float maxWidth)
        {
            return optionToggled ? 310 : 150f;
        }

        public override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
        {
            Rect MainRect = new Rect(topLeft.x, topLeft.y, 150, 75f);
            Find.WindowStack.ImmediateWindow(145356798, MainRect, WindowLayer.GameUI, delegate
            {
                Rect rect = MainRect.AtZero().ContractedBy(5f);
                Rect optionRect = new Rect(rect.xMax - 15, rect.y, 15, 15);
                bool mouseOver = Mouse.IsOver(rect);
                GUI.color = mouseOver ? Color.cyan : Color.white;
                Widgets.DrawTextureFitted(optionRect, TiberiumContent.InfoButton, 1f);
                GUI.color = Color.white;
                if (Widgets.ButtonInvisible(rect))
                    optionToggled = !optionToggled;

                Text.Font = GameFont.Tiny;
                Text.Anchor = TextAnchor.UpperCenter;
                Widgets.Label(rect, "TR_ContainerContent".Translate());
                Text.Anchor = TextAnchor.MiddleCenter;
                Text.Font = GameFont.Small;
                Widgets.Label(rect, Math.Round(container.TotalStorage, 0) + "/" + container.TotalCapacity);
                Text.Anchor = 0;
                Rect rect2 = rect.BottomHalf();
                Rect rect3 = rect2.BottomHalf();
                GUI.BeginGroup(rect3);
                Rect BGRect = new Rect(0, 0, rect3.width, rect3.height);
                Rect BarRect = BGRect.ContractedBy(2.5f);
                float xPos = 0f;
                Widgets.DrawBoxSolid(BGRect, new Color(0.05f, 0.05f, 0.05f));
                Widgets.DrawBoxSolid(BarRect, new Color(0.25f, 0.25f, 0.25f));
                foreach (TiberiumValueType type in container.AllStoredTypes)
                {
                    float percent = (container.ValueForType(type) / container.TotalCapacity);
                    Rect typeRect = new Rect(2.5f + xPos, BarRect.y, BarRect.width * percent, BarRect.height);
                    Color color = TRUtils.GetColor(type);
                    xPos += BarRect.width * percent;
                    Widgets.DrawBoxSolid(typeRect, color);
                }

                GUI.EndGroup();
                if (optionToggled)
                {
                    Rect Main2 = new Rect(topLeft.x + 160, topLeft.y, 150, 75f);
                    DrawOptions(Main2);
                }

            }, true, false, 1f);
            return new GizmoResult(GizmoState.Clear);
        }

        public void DrawOptions(Rect inRect)
        {
            Find.WindowStack.ImmediateWindow(1453564358, inRect, WindowLayer.GameUI, delegate
            {

            }, true, false, 1f);
        }
    }
}
