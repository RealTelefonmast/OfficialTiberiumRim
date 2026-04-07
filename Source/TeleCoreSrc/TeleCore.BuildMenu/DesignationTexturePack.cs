using UnityEngine;
using Verse;

namespace TeleCore.BuildMenu;

/// <summary>
///     The texture pack provides textures for the custom build menu
/// </summary>
public class DesignationTexturePack
{
    private static readonly string DefaultPackPath = "Menu/SubBuildMenu";

    //
    public Texture2D backGround;
    public Texture2D designator;
    public Texture2D designatorSelected;
    public Texture2D tab;
    public Texture2D tabSelected;


    public DesignationTexturePack(string packPath, Def fromDef)
    {
        packPath ??= DefaultPackPath;
        backGround = ContentFinder<Texture2D>.Get(packPath + "/BuildMenu");
        tab = ContentFinder<Texture2D>.Get(packPath + "/Tab");
        tabSelected = ContentFinder<Texture2D>.Get(packPath + "/Tab_Sel");
        designator = ContentFinder<Texture2D>.Get(packPath + "/Des");
        designatorSelected = ContentFinder<Texture2D>.Get(packPath + "/Des_Sel");
    }
}