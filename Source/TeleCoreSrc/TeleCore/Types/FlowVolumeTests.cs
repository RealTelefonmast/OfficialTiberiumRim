using System.Collections.Generic;
using System.Linq;
using TeleCore.Defs;
using TeleCore.Types.Exposables;
using UnityEngine;
using UnityEngine.Assertions;

namespace TeleCore.Types;

[TestFixture]
public class FlowVolumeTests
{
    public static readonly FlowValueDef[] defs = new FlowValueDef[2]
    {
        new()
        {
            defName = "ValueA",
            label = "value A",
            labelShort = "a",
            valueUnit = "°",
            valueColor = Color.red,
            viscosity = 1
        },
        new()
        {
            defName = "ValueB",
            label = "value B",
            labelShort = "b",
            valueUnit = "°",
            valueColor = Color.blue,
            viscosity = 1
        }
    };

    private static List<FlowVolume<FlowValueDef>> volumes;


    public static FlowVolumeConfig<FlowValueDef> Config => new()
    {
        AllowedValues = new List<FlowValueDef> { defs[0], defs[1] },
        capacity = 500
    };

    [SetUp]
    public void Setup()
    {
        volumes = new List<FlowVolume<FlowValueDef>>
        {
            new(Config),
            new(Config)
        };
    }

    [Test]
    public void AdditionTest()
    {
        var res1 = volumes[0].TryAdd(defs[0], 250);
        var res2 = volumes[0].TryAdd(defs[1], 250);

        Assert.AreEqual(250d, res1.Desired.Value);
        Assert.AreEqual(250d, res2.Desired.Value);
        Assert.IsTrue(res1.State == FlowState.Completed);
        Assert.IsTrue(res2.State == FlowState.Completed);

        Assert.AreEqual(250, res1.Actual.Value);
        Assert.AreEqual(250, res2.Actual.Value);

        Assert.AreEqual(250, volumes[0].StoredValueOf(defs[0]));
        Assert.AreEqual(250, volumes[0].StoredValueOf(defs[1]));
        Assert.AreEqual(500, volumes[0].TotalValue);
    }

    [Test]
    public void AdditionTest_AdhereToCpacity()
    {
        var volume = new FlowVolume<FlowValueDef>(new FlowVolumeConfig<FlowValueDef>
        {
            AllowedValues = defs.ToList(),
            capacity = 250
        });

        Assert.AreEqual(250, volume.MaxCapacity);

        var res1 = volume.TryAdd(defs[0], 125);
        var res2 = volume.TryAdd(defs[1], 100);
        var res3 = volume.TryAdd(defs[1], 50);

        Assert.AreEqual(1.0f, volume.FillPercent);
        Assert.IsTrue(volume.Full);
        Assert.AreEqual(FlowState.CompletedWithExcess, res3.State);

        var res4 = volume.TryAdd(defs[0], 125);

        Assert.IsTrue(volume.Full);
        Assert.AreEqual(250, volume.TotalValue);
        Assert.AreEqual(FlowState.Failed, res4.State);
    }

    [Test]
    public void SubtractionTest_Expected()
    {
        const double initialValue = 250;
        const double expectedValue = 125;

        var volume = volumes[0];

        var addRes1 = volume.TryAdd(defs[0], initialValue);
        var addRes2 = volume.TryAdd(defs[1], initialValue);

        volume.TryRemove(defs[0], addRes1.Actual / 2);
        volume.TryRemove(defs[1], addRes2.Actual / 2);

        Assert.AreEqual(expectedValue, volume.StoredValueOf(defs[0]));
        Assert.AreEqual(expectedValue, volume.StoredValueOf(defs[1]));
        Assert.AreEqual(2 * expectedValue, volume.TotalValue);
    }

    [Test]
    public void SubtractionTest_Empty()
    {
        var subResEmpty = volumes[1].TryRemove(defs[0], 100);
        Assert.IsTrue(subResEmpty.State == FlowState.Failed);
    }

    [Test]
    public void TransferTest_FullToEmpty()
    {
        var res1 = volumes[0].TryAdd(defs[0], 250);
        var res2 = volumes[0].TryAdd(defs[1], 250);
        var res3 = volumes[0].TryTransfer(volumes[1], (defs[0], 250));
        var res4 = volumes[0].TryTransfer(volumes[1], (defs[1], 250));

        Assert.AreEqual(FlowState.Completed, res1.State);
        Assert.AreEqual(FlowState.Completed, res2.State);
        Assert.AreEqual(FlowState.Completed, res3.State);
        Assert.AreEqual(FlowState.Completed, res4.State);

        Assert.AreEqual(250, volumes[1].StoredValueOf(defs[0]));
        Assert.AreEqual(250, volumes[1].StoredValueOf(defs[1]));
        Assert.AreEqual(500, volumes[1].TotalValue);
        Assert.IsTrue(volumes[0].Empty);
        Assert.IsTrue(volumes[1].Full);
    }

    [Test]
    public void TransferTest_ExactNoOverflow()
    {
        var res1 = volumes[0].TryAdd(defs[0], 250);
        var res2 = volumes[0].TryAdd(defs[1], 250);

        var res2_1 = volumes[1].TryAdd(defs[1], 125);
        var res3 = volumes[0].TryTransfer(volumes[1], (defs[0], 250));
        var res4 = volumes[0].TryTransferOrFail(volumes[1], (defs[1], 250));

        Assert.AreEqual(FlowState.Completed, res1.State);
        Assert.AreEqual(FlowState.Completed, res2.State);
        Assert.AreEqual(FlowState.Completed, res3.State);
        Assert.AreEqual(FlowState.Failed, res4.State);

        Assert.AreEqual(250, res4.Diff.Value);

        Assert.AreEqual(250, volumes[1].StoredValueOf(defs[0]));
        Assert.AreEqual(125, volumes[1].StoredValueOf(defs[1]));
        Assert.AreEqual(250 + 125, volumes[1].TotalValue);
        Assert.IsTrue(!volumes[0].Empty);
        Assert.IsTrue(!volumes[1].Full);
    }

    [Test]
    public void TransferTest_TryWithOverflow()
    {
        var res1 = volumes[0].TryAdd(defs[0], 250);
        var res2 = volumes[0].TryAdd(defs[1], 250);

        var res2_1 = volumes[1].TryAdd(defs[1], 125);
        var res3 = volumes[0].TryTransfer(volumes[1], (defs[0], 250));
        var res4 = volumes[0].TryTransfer(volumes[1], (defs[1], 250));

        Assert.AreEqual(FlowState.Completed, res1.State);
        Assert.AreEqual(FlowState.Completed, res2.State);
        Assert.AreEqual(FlowState.Completed, res3.State);
        Assert.AreEqual(FlowState.CompletedWithExcess, res4.State);

        Assert.AreEqual(125, res4.Diff.Value);

        Assert.AreEqual(250, volumes[1].StoredValueOf(defs[0]));
        Assert.AreEqual(250, volumes[1].StoredValueOf(defs[1]));
        Assert.AreEqual(500, volumes[1].TotalValue);
        Assert.IsTrue(volumes[0].Empty);
        Assert.IsTrue(volumes[1].Full);
    }

    [Test]
    public void ContentTest()
    {
        //Fill and remove
        var addRes1 = volumes[0].TryAdd(defs[0], 250);
        var remTest = volumes[0].RemoveContent(125d);

        //Remove from empty
        var remTest2 = volumes[1].RemoveContent(100);

        Assert.AreEqual(125d, volumes[0].TotalValue);
        Assert.IsTrue(remTest2.IsEmpty);
    }
}

/*
[TestFixture]
public class FlowVolumeTests
{
    private static List<FlowVolume<FlowValueDef>> volumes;

    public static readonly FlowValueDef[] defs = new FlowValueDef[2]
    {
        new FlowValueDef
        {
            defName = "ValueA",
            label = "value A",
            labelShort = "a",
            valueUnit = "°",
            valueColor = Color.red,
            viscosity = 1,
        },
        new FlowValueDef
        {
            defName = "ValueB",
            label = "value B",
            labelShort = "b",
            valueUnit = "°",
            valueColor = Color.blue,
            viscosity = 1,
        },
    };

    public static FlowVolumeConfig<FlowValueDef> Config => new FlowVolumeConfig<FlowValueDef>
    {
        //allowedValues = new List<FlowValueDef>() { defs[0], defs[1]},
        capacity = 500,
    };

    [OneTimeSetUp]
    public void Setup()
    {
        volumes = new List<FlowVolume<FlowValueDef>>
        {
            new FlowVolume<FlowValueDef>(Config),
            new FlowVolume<FlowValueDef>(Config)
        };
    }

    [Test]
    public void AdditionTest()
    {
        var res1 = volumes[0].TryAdd(defs[0], 250);
        var res2 = volumes[0].TryAdd(defs[1], 250);

        Assert.AreEqual(250d ,res1.Desired.Value);
        Assert.AreEqual(250d ,res2.Desired.Value);
        Assert.IsTrue(res1.State == FlowState.Completed);
        Assert.IsTrue(res2.State == FlowState.Completed);

        Assert.AreEqual(250, res1.Actual);
        Assert.AreEqual(250, res2.Actual);

        Assert.AreEqual(250, volumes[0].StoredValueOf(defs[0]));
        Assert.AreEqual(250, volumes[0].StoredValueOf(defs[1]));
        Assert.AreEqual(500, volumes[0].TotalValue);
    }

    [Test]
    public void AdditionTest_AdhereToCpacity()
    {
        var volume = new FlowVolume<FlowValueDef>(new FlowVolumeConfig<FlowValueDef>
        {
            //allowedValues = defs.ToList(),
            capacity = 250,
        });

        Assert.AreEqual(250, volume.MaxCapacity);

        var res1 = volume.TryAdd(defs[0], 125);
        var res2 = volume.TryAdd(defs[1], 100);
        var res3 = volume.TryAdd(defs[1], 50);

        Assert.AreEqual(1.0d, volume.FillPercent);
        Assert.IsTrue(volume.Full);
        Assert.AreEqual(FlowState.CompletedWithExcess, res3.State);

        var res4 = volume.TryAdd(defs[0], 125);

        Assert.IsTrue(volume.Full);
        Assert.AreEqual(250, volume.TotalValue);
        Assert.AreEqual(FlowState.Failed, res4.State);
    }

    [Test]
    public void SubtractionTest_Expected()
    {
        const double initialValue = 250;
        const double expectedValue = 125;

        var volume = volumes[0];

        var addRes1 = volume.TryAdd(defs[0], initialValue);
        var addRes2 = volume.TryAdd(defs[1], initialValue);

        volume.TryRemove(defs[0], addRes1.Actual / 2);
        volume.TryRemove(defs[1], addRes2.Actual / 2);

        Assert.AreEqual(expectedValue, volume.StoredValueOf(defs[0]));
        Assert.AreEqual(expectedValue, volume.StoredValueOf(defs[1]));
        Assert.AreEqual(2 * expectedValue, volume.TotalValue);
    }

    [Test]
    public void SubtractionTest_Empty()
    {
        var subResEmpty = volumes[1].TryRemove(defs[0], 100);
       Assert.IsTrue(subResEmpty.Actual == 0 && subResEmpty.State == FlowState.Failed);
    }

    [Test]
    public void ContentTest()
    {
        //Fill and remove
        var addRes1 = volumes[0].TryAdd(defs[0], 250);
        var remTest = volumes[0].RemoveContent(125);

        //Remove from empty
        var remTest2 = volumes[1].RemoveContent(100);

        Assert.AreEqual(125, volumes[0].TotalValue);
        Assert.IsTrue(remTest2.IsEmpty);
    }
}
*/