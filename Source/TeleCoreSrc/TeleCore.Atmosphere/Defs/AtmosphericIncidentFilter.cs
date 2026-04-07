using System.Xml;
using RimWorld;
using TeleCore.Logging;
using TeleCore.Utils;
using Verse;

namespace TeleCore.Atmosphere.Defs;

public class AtmosphericIncidentFilter : Editable
{
    public IncidentDef incidentDef;
    public AtmosphericValueDef AtmosValueDef;
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
