using UnityEngine;
using Verse;

namespace TeleCore.UI;

public class GUIStyleStateDef : Def
{
    public string backgroundImagePath = null;
    public Color textColor = Color.white;


    public GUIStyleState State => GetState();

    public GUIStyleState GetState() => new GUIStyleState
    {
        background = backgroundImagePath != null ? ContentFinder<Texture2D>.Get(backgroundImagePath) : null,
        textColor = textColor
    };
}