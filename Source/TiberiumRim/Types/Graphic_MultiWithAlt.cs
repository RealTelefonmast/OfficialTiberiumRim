using UnityEngine;
using Verse;

namespace TR;

public class Graphic_MultiWithAlt : Graphic_Multi
{
    public const string AlternateSuffix = "_alt";
    private readonly Material[] matsAlt = new Material[4];

    public Material MatWestAlt => matsAlt[Rot4.WestInt] != null ? matsAlt[Rot4.WestInt] : MatWest;
    public Material MatEastAlt => matsAlt[Rot4.EastInt] != null ? matsAlt[Rot4.EastInt] : MatEast;
    public Material MatNorthAlt => matsAlt[Rot4.NorthInt] != null ? matsAlt[Rot4.NorthInt] : MatNorth;
    public Material MatSouthAlt => matsAlt[Rot4.SouthInt] != null ? matsAlt[Rot4.SouthInt] : MatSouth;

    public override Material MatAt(Rot4 rot, Thing thing = null)
    {
        if (thing != null && thing.def.TeleExtension().AlternateGraphicWorker.NeedsAlt(rot, thing))
            return rot.AsInt switch
            {
                0 => MatNorthAlt,
                1 => MatEastAlt,
                2 => MatSouthAlt,
                3 => MatWestAlt,
                _ => BaseContent.BadMat
            };
        return base.MatAt(rot, thing);
    }

    public override void Init(GraphicRequest req)
    {
        base.Init(req);
        var array = new Texture2D[mats.Length];
        array[0] = ContentFinder<Texture2D>.Get(req.path + "_north" + AlternateSuffix, false);
        array[1] = ContentFinder<Texture2D>.Get(req.path + "_east" + AlternateSuffix, false);
        array[2] = ContentFinder<Texture2D>.Get(req.path + "_south" + AlternateSuffix, false);
        array[3] = ContentFinder<Texture2D>.Get(req.path + "_west" + AlternateSuffix, false);

        //
        if (array[0] == null)
        {
            if (array[2] != null)
            {
                array[0] = array[2];
                drawRotatedExtraAngleOffset = 180f;
            }
            else if (array[1] != null)
            {
                array[0] = array[1];
                drawRotatedExtraAngleOffset = -90f;
            }
            else if (array[3] != null)
            {
                array[0] = array[3];
                drawRotatedExtraAngleOffset = 90f;
            }
            else
            {
                array[0] = ContentFinder<Texture2D>.Get(req.path, false);
            }
        }

        if (array[0] == null)
        {
            Log.Error("Failed to find any textures at " + req.path + " while constructing " +
                      this.ToStringSafe<Graphic_Multi>());
            return;
        }

        if (array[2] == null) array[2] = array[0];
        if (array[1] == null)
        {
            if (array[3] != null)
            {
                array[1] = array[3];
                eastFlipped = DataAllowsFlip;
            }
            else
            {
                array[1] = array[0];
            }
        }

        if (array[3] == null)
        {
            if (array[1] != null)
            {
                array[3] = array[1];
                westFlipped = DataAllowsFlip;
            }
            else
            {
                array[3] = array[0];
            }
        }

        //
        for (var i = 0; i < mats.Length; i++)
        {
            var req2 = default(MaterialRequest);
            req2.mainTex = array[i];
            req2.shader = req.shader;
            req2.color = color;
            req2.colorTwo = colorTwo;
            req2.shaderParameters = req.shaderParameters;
            req2.renderQueue = req.renderQueue;
            matsAlt[i] = MaterialPool.MatFrom(req2);
        }
    }
}