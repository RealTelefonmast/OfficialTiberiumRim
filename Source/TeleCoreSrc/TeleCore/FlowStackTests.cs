using System.Collections.Generic;
using TeleCore.FlowCore;
using TeleCore.Primitive;
using UnityEngine.Assertions;

namespace TeleCore;

[TestFixture]
public class FlowStackTests
{
    public static List<FlowValueDef> Defs;

    [SetUp]
    public void Setup()
    {
        Defs = new List<FlowValueDef>();
        Defs.Add(new FlowValueDef
        {
            defName = "ValueA",
            label = "val a",
            labelShort = "(a)",
            valueUnit = "p",
            viscosity = 1,
            capacityFactor = 1,
        });
        Defs.Add(new FlowValueDef
        {
            defName = "ValueB",
            label = "val b",
            labelShort = "(b)",
            valueUnit = "p",
            viscosity = 1,
            capacityFactor = 1,
        });
        Defs.Add(new FlowValueDef
        {
            defName = "ValueC",
            label = "val c",
            labelShort = "(c)",
            valueUnit = "p",
            viscosity = 1,
            capacityFactor = 1,
        });
    }
    
    [Test]
    public void Mutability()
    {
        DefValueStack<FlowValueDef, double> stack = new ();
        stack += new DefValue<FlowValueDef, double>(Defs[0], 1);
        stack += new DefValue<FlowValueDef, double>(Defs[1], 33);
        stack += new DefValue<FlowValueDef, double>(Defs[2], 66);
        var stck2 = stack + stack;
        
        Assert.AreNotEqual(stack, stck2);
    }
    
    [Test]
    public void TotalValue()
    {
        DefValueStack<FlowValueDef, double> stack = new ();
        stack += new DefValue<FlowValueDef, double>(Defs[0], 1);
        stack += new DefValue<FlowValueDef, double>(Defs[1], 33);
        stack += new DefValue<FlowValueDef, double>(Defs[2], 66);
        var stck2 = stack + stack;
        
        Assert.AreEqual(stack.TotalValue.Value, 1 + 33 + 66);
        Assert.AreEqual(stck2.TotalValue, stack.TotalValue + stack.TotalValue);
    }
}