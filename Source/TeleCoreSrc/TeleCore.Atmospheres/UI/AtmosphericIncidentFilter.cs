using System.Xml;
using RimWorld;
using TeleCore.Defs;
using TeleCore.Types.Utils;
using Verse;

namespace TeleCore.Atmospheres.UI;

public class AtmosphericIncidentFilter : Editable
{
    public AtmosphericValueDef AtmosValueDef;
    public IncidentDef incidentDef;
    public float threshold = 0;

    public override void PostLoad()
    {
        base.PostLoad();
        TLog.Message("Postloading");
    }

    // IncidentDef -> >(AtmosDef, 0.5)
    public void LoadDataFromXmlCustom(XmlNode xmlRoot)
    {
    }
}