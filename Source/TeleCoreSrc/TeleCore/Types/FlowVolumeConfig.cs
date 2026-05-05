using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using Verse;

namespace TeleCore.Types;

public class FlowVolumeConfig<T> where T : FlowValueDef
{
    private readonly List<T> _values = new();
    public int area = 1;
    public int capacity;
    public bool dropContents;
    public int elevation = 0;
    public int height = 1;

    public bool infiniteSource = false;
    private bool isReady;
    public bool leaveContainer;
    public bool shareCapacity;
    public bool storeEvenly;

    public Values values;

    public float Volume => capacity;

    public IReadOnlyCollection<T> AllowedValues
    {
        get
        {
            if (isReady) return _values;
            if (values == null) return _values;

            //Prepare
            if (!values.allowedValues.NullOrEmpty())
                _values.AddRange(values.allowedValues);
            if (values.fromCollection != null)
                foreach (var var in values.fromCollection.ValueDefs.Cast<T?>())
                    _values.Add(var);

            isReady = true;
            return _values;
        }
        set => _values.AddRange(value);
    }

    public void PostLoad()
    {
    }

    public class Values
    {
        public List<T> allowedValues;
        public FlowValueCollectionDef fromCollection;
    }

    //Note:We dont need this approach for now
    //public double Volume => area * height * AREA_VALUE;
}