using UnityEngine;
using Verse;

namespace TeleCore.BuildMenu;

[StaticConstructorOnStartup]
public static class BuildMenuContent
{
    public static readonly Texture2D Undiscovered = ContentFinder<Texture2D>.Get("UI/Menu/Undiscovered");
    public static readonly Texture2D Favorite_Filled = ContentFinder<Texture2D>.Get("UI/Menu/Star_Filled");

    public static readonly Texture2D Favorite_Unfilled = ContentFinder<Texture2D>.Get("UI/Menu/Star_Unfilled");
}