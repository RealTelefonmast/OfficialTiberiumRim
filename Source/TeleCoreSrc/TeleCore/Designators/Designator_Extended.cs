using TeleCore.Unsorted;
using UnityEngine;
using Verse;

namespace TeleCore.Designators;

[StaticConstructorOnStartup]
public abstract class Designator_Extended : Designator
{
    private static readonly Texture2D cooldownBarTex = SolidColorMaterials.NewSolidColorTexture(new Color32(180, 180, 180, 64));
    public ICoolDownHolder coolDown;

    protected bool mustBeUsed = false;

    public virtual bool MustStaySelected => mustBeUsed;


    public sealed override GizmoResult GizmoOnGUI(Vector2 topLeft, float maxWidth, GizmoRenderParms parms)
    {
        var onCooldown = CooldownCheck();
        var result = DrawGizmo(topLeft, maxWidth, parms, onCooldown); ;
        if (onCooldown)
        {
            Rect rect = new Rect(topLeft.x, topLeft.y, GetWidth(maxWidth), 75f);
            Widgets.FillableBar(rect, Mathf.Clamp01(coolDown.DisabledPct), cooldownBarTex, null, false);

            //
            Text.Font = GameFont.Tiny;
            Text.Anchor = TextAnchor.UpperCenter;
            Widgets.Label(rect, coolDown.DisabledPct.ToStringPercent("F0"));
            Text.Anchor = TextAnchor.UpperLeft;
        }

        return new GizmoResult(result.State);
    }

    private bool CooldownCheck()
    {
        this.disabled = coolDown is { CoolDownActive: true };
        return disabled;
    }

    public virtual GizmoResult DrawGizmo(Vector2 topLeft, float maxWidth, GizmoRenderParms parms, bool onCooldown)
    {
        return base.GizmoOnGUI(topLeft, maxWidth, parms);
    }
}