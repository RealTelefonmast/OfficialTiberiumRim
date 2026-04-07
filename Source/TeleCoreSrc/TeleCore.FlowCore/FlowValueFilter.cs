using System.Collections.Generic;

namespace TeleCore.FlowCore;

public class FlowValueFilter<TValue>
where TValue : FlowValueDef
{
    public List<TValue> allowedValues;
}