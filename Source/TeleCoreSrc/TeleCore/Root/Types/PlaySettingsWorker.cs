using UnityEngine;
using Verse;

namespace TeleCore.Types;

/// <summary>
///     Provides an entry point to add a custom PlaySettings Option
/// </summary>
public abstract class PlaySettingsWorker
{
    protected PlaySettingsWorker()
    {
        Active = DefaultValue;
    }

    public abstract bool Visible { get; }
    public bool Active { get; internal set; }

    public virtual bool ShowOnWorldView { get; } = false;
    public virtual bool ShowOnMapView { get; } = true;

    public virtual bool DefaultValue { get; } = false;

    public abstract Texture2D Icon { get; }

    public Texture2D ActiveIcon => Icon.NullOrBad() ? BaseContent.BadTex : Icon;

    public abstract string Description { get; }

    public abstract void OnToggled(bool isActive);

    internal void Toggle()
    {
        Active = !Active;
        OnToggled(Active);
    }
}