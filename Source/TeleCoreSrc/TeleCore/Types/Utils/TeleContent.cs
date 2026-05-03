using RimWorld;
using UnityEngine;
using Verse;

namespace TeleCore.Unsorted;

[StaticConstructorOnStartup]
public static class TeleContent
{
    //UI
    public static readonly Texture2D InfoButton = ContentFinder<Texture2D>.Get("UI/Buttons/InfoButton");
    public static readonly Texture2D SideBarArrow = ContentFinder<Texture2D>.Get("UI/Icons/Arrow");

    //
    public static readonly Texture2D ButtonBGAtlas = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonBG");

    public static readonly Texture2D ButtonBGAtlasMouseover =
        ContentFinder<Texture2D>.Get("UI/Buttons/ButtonBGMouseover");

    public static readonly Texture2D ButtonBGAtlasClick = ContentFinder<Texture2D>.Get("UI/Buttons/ButtonBGClick");

    //
    public static readonly Texture2D EdgeArrow = ContentFinder<Texture2D>.Get("UI/DebugIcons/EdgeArrow");

    //General
    public static readonly Texture2D HightlightInMenu = ContentFinder<Texture2D>.Get("UI/Icons/HighLight");
    public static readonly Texture2D OpenMenu = ContentFinder<Texture2D>.Get("UI/Icons/OpenMenu");

    public static Texture2D CustomSlider = ContentFinder<Texture2D>.Get("UI/Icons/CustomSlider");

    //SubBuildMenu
    public static readonly Texture2D Undiscovered = ContentFinder<Texture2D>.Get("UI/Menu/Undiscovered");
    public static readonly Texture2D Favorite_Filled = ContentFinder<Texture2D>.Get("UI/Menu/Star_Filled");

    public static readonly Texture2D
        Favorite_Unfilled = ContentFinder<Texture2D>.Get("UI/Menu/Star_Unfilled");

    //UIElement
    public static readonly Texture2D LockOpen = ContentFinder<Texture2D>.Get("UI/Icons/Animator/LockOpen");

    public static readonly Texture2D
        LockClosed = ContentFinder<Texture2D>.Get("UI/Icons/Animator/LockClosed");

    public static readonly Texture2D UIDataNode = ContentFinder<Texture2D>.Get("UI/Icons/Tools/Node");

    //TextureElement
    public static readonly Texture2D
        PivotPoint = ContentFinder<Texture2D>.Get("UI/Icons/Animator/PivotPoint");

    //Animation Tool
    //LayerView
    public static readonly Texture2D LinkIcon = ContentFinder<Texture2D>.Get("UI/Icons/Link", false);
    public static readonly Texture2D VisibilityOff = ContentFinder<Texture2D>.Get("UI/Icons/VisibilityOff", false);
    public static readonly Texture2D VisibilityOn = ContentFinder<Texture2D>.Get("UI/Icons/VisibilityOn", false);

    public static readonly Texture2D BurgerMenu = ContentFinder<Texture2D>.Get("UI/Icons/BurgerMenu", false);
    public static readonly Texture2D SettingsWheel = ContentFinder<Texture2D>.Get("UI/Icons/SettingsWheel", false);
    public static readonly Texture2D HelpIcon = ContentFinder<Texture2D>.Get("UI/Icons/Help", false);

    //TimeLine
    public static readonly Texture2D KeyFrame = ContentFinder<Texture2D>.Get("UI/Icons/Animator/KeyFrame");

    public static readonly Texture2D KeyFrameSelection =
        ContentFinder<Texture2D>.Get("UI/Icons/Animator/KeyFrameSelection");

    public static readonly Texture2D AddKeyFrame =
        ContentFinder<Texture2D>.Get("UI/Icons/Animator/AddKeyFrame");

    public static readonly Texture2D TimeSelMarker =
        ContentFinder<Texture2D>.Get("UI/Icons/Animator/TimeSelector");

    public static readonly Texture2D PlayPause = ContentFinder<Texture2D>.Get("UI/Icons/Animator/PlayPause");
    public static readonly Texture2D TimeSelRangeL = ContentFinder<Texture2D>.Get("UI/Icons/Animator/RangeL");
    public static readonly Texture2D TimeSelRangeR = ContentFinder<Texture2D>.Get("UI/Icons/Animator/RangeR");

    internal static readonly Texture2D TimeFlag = ContentFinder<Texture2D>.Get("UI/Icons/Animator/FlagAtlas");

    //ModuleVis
    public static readonly Texture Node_Open = ContentFinder<Texture2D>.Get("UI/Icons/Node_Open");
    public static readonly Texture NodeOut_Closed = ContentFinder<Texture2D>.Get("UI/Icons/NodeOut_Closed");
    public static readonly Texture NodeIn_Closed = ContentFinder<Texture2D>.Get("UI/Icons/NodeIn_Closed");

    //Internal RW Crap /FROM TexButton
    public static readonly Texture2D DeleteX = TexButton.Delete;
    public static readonly Texture2D Plus = TexButton.Plus;
    public static readonly Texture2D Minus = TexButton.Minus;
    public static readonly Texture2D Infinity = TexButton.Infinity;
    public static readonly Texture2D Suspend = TexButton.Suspend;
    public static readonly Texture2D Copy = TexButton.Copy;
    public static readonly Texture2D Paste = TexButton.Paste;

    //Materials
    public static readonly Material ForcedTargetLineMat =
        MaterialPool.MatFrom(GenDraw.LineTexPath, ShaderDatabase.Transparent, new Color(1f, 0.5f, 0.5f));

    public static readonly Material IOArrow = MaterialPool.MatFrom("Buildings/IO/IOArrow", ShaderDatabase.Transparent);

    public static readonly Material IOArrowRed =
        MaterialPool.MatFrom("Buildings/IO/IOArrow", ShaderDatabase.Transparent, Color.red);

    public static readonly Material IOArrowTwoWay =
        MaterialPool.MatFrom("Buildings/IO/IOArrowTwoWay", ShaderDatabase.Transparent);

    public static readonly Material IOArrowLogical =
        MaterialPool.MatFrom("Buildings/IO/IOArrowTwoWay", ShaderDatabase.Transparent, Color.red);

    public static readonly Material WorldTerrain =
        MaterialPool.MatFrom("World/Tile/Terrain", ShaderDatabase.WorldOverlayCutout, 3500);


    public static readonly Material ClearTextureMat = MaterialPool.MatFrom("Blank", ShaderDatabase.Transparent);

    //Graphics
    public static readonly Graphic_Single ClearGraphic = new()
    {
        data = new GraphicData
        {
            texPath = "NoPath",
            graphicClass = typeof(Graphic_Single),
            shaderType = ShaderTypeDefOf.Transparent,
            renderInstanced = true,
            allowAtlasing = false
        },
        path = "NoPath",
        mat = ClearTextureMat
    };
}