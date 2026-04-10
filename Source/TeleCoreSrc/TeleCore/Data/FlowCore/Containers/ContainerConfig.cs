using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using TeleCore.GameData.Defs.Extensions;
using Verse;

namespace TeleCore.FlowCore.Containers;

public class ContainerConfig<TDef> : IExposable
    where TDef : FlowValueDef
{
    [Unsaved] private List<TDef> allowedValuesInt = null!;

    public int baseCapacity;

    public Type containerClass = typeof(ValueContainerBase<TDef, double>);
    public string containerLabel;
    public bool dropContents;

    //Assumed for Networks only
    public bool leaveContainer;

    //TODO: 
    public bool storeEvenly; // ReSharper disable local UnassignedField.Global
    public ThingDef? droppedContainerDef;
    public ExplosionProperties? explosionProps;
    public FlowValueFilterSettings? defaultFilterSettings;

    //
    public FlowValueDefFilter valueDefs;

    public List<TDef> AllowedValues
    {
        get
        {
            if (allowedValuesInt == null)
            {
                var list = new List<TDef>();
                if (valueDefs != null)
                {
                    if (valueDefs.fromCollection != null)
                        list.AddRange(valueDefs.fromCollection.ValueDefs.Cast<TDef>());
                    if (!valueDefs.values.NullOrEmpty()) list.AddRange(valueDefs.values);
                }

                allowedValuesInt = list.Distinct().ToList();
            }

            return allowedValuesInt;
        }
    }

    public sealed class FlowValueDefFilter : IExposable
    {
        public FlowValueCollectionDef fromCollection;
        public List<TDef> values;

        public void ExposeData()
        {
            Scribe_Defs.Look(ref fromCollection, nameof(fromCollection));
            Scribe_Collections.Look(ref values, nameof(values), LookMode.Def);
        }

        public static implicit operator FlowValueDefFilter(List<TDef> values)
        {
            return new FlowValueDefFilter
            {
                values = values
            };
        }

        public void LoadDataFromXmlCustom(XmlNode xmlRoot)
        {
            if (xmlRoot.FirstChild.Name == "li")
            {
                values = DirectXmlToObject.ObjectFromXml<List<TDef>>(xmlRoot, true);
                return;
            }

            var fromDefNode = xmlRoot.SelectSingleNode(nameof(fromCollection));
            if (fromDefNode != null)
                DirectXmlCrossRefLoader.RegisterObjectWantsCrossRef(this, $"{nameof(fromCollection)}",
                    fromDefNode.InnerText);

            //
            var valuesNode = xmlRoot.SelectSingleNode(nameof(values));
            if (valuesNode != null) values = DirectXmlToObject.ObjectFromXml<List<TDef>>(valuesNode, true);
        }
    }

    public void ExposeData()
    {
        Scribe_Values.Look(ref baseCapacity, nameof(baseCapacity));
        Scribe_Values.Look(ref containerLabel, nameof(containerLabel));
        Scribe_Values.Look(ref storeEvenly, nameof(storeEvenly));
        Scribe_Values.Look(ref dropContents, nameof(dropContents));
        Scribe_Values.Look(ref leaveContainer, nameof(leaveContainer));
        Scribe_Deep.Look(ref valueDefs, nameof(valueDefs));
        Scribe_Deep.Look(ref explosionProps, nameof(explosionProps));
    }

    public ContainerConfig<TDef> Copy()
    {
        return new ContainerConfig<TDef>
        {
            containerClass = containerClass,
            baseCapacity = baseCapacity,
            storeEvenly = storeEvenly,
            dropContents = dropContents,
            leaveContainer = leaveContainer,
            explosionProps = explosionProps
        };
    }
}