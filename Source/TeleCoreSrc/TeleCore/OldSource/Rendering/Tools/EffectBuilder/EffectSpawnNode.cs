using TeleCore.Rendering.UI.DynaUI;
using UnityEngine;

namespace TeleCore.Rendering.Tools.EffectBuilder;

internal class EffectSpawnNode : UIElement
{
    public EffectSpawnNode(UIElementMode mode) : base(mode)
    {
    }

    public EffectSpawnNode(Rect rect, UIElementMode mode) : base(rect, mode)
    {
    }

    public EffectSpawnNode(Vector2 pos, Vector2 size, UIElementMode mode) : base(pos, size, mode)
    {
    }
}