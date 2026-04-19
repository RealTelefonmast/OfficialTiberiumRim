using System.Text.RegularExpressions;
using System.Xml;
using Verse;

namespace TiberiumRim;

public class TerrainFloat
{
    public TerrainDef terrainDef;
    public float value = 1f;

    public TerrainFloat()
    {
    }

    public TerrainFloat(TerrainDef terrainDef, float value)
    {
        this.terrainDef = terrainDef;
        this.value = value;
    }

    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
        var s = Regex.Replace(xmlRoot.FirstChild.Value, @"\s+", "");
        var array = s.Split(new[]
        {
            ','
        });
        terrainDef = DefDatabase<TerrainDef>.GetNamed(array[0]);
        value = (float)ParseHelper.FromString(array[1], typeof(float));
    }
}