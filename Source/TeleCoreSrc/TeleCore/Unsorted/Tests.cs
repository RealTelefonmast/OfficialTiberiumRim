using UnityEngine;

namespace TeleCore.Unsorted
{
    [TestFixture]
    public class Tests
    {
        
        [Test]
        public void AngleTests()
        {
            for (int x = -10; x < 10; x++)
            {
                for(int y = -10; y < 10; y++)
                {
                    float angle = Mathf.Atan2(1, 0) * Mathf.Rad2Deg;
                    angle = TMath.AngleWrapped(angle - 90f);
                }
            }
        }

    }
    /*
    [TestFixture]
    public class Tests
    {
        public static NetworkDef[] NetworkDefs = new NetworkDef[1]
        {
            new NetworkDef
            {
                defName = "TestNetworkDef",
                containerLabel = "ContainerLabel",
                transmitterGraphic = null,
                overlayGraphic = null,
                controllerDef = null,
                transmitterDef = null
            },
        };
        

        public static NetworkValueDef[] ValueDefs = new NetworkValueDef[2]
        {
            new NetworkValueDef
            {
                defName = "Value1",
                label = null,
                description = null,
                descriptionHyperlinks = null,
                ignoreConfigErrors = false,
                ignoreIllegalLabelCharacterConfigError = false,
                modExtensions = null,
                shortHash = 0,
                index = 0,
                modContentPack = null,
                fileName = null,
                generated = false,
                debugRandomId = 0,
                labelShort = null,
                valueUnit = null,
                valueColor = default,
                sharesCapacity = false,
                viscosity = 0,
                capacityFactor = 0,
                networkDef = null,
                specialDroppedContainerDef = null,
                thingDroppedFromContainer = null,
                valueToThingRatio = 0
            },
            new NetworkValueDef
            {
                defName = "Value2",
                label = null,
                description = null,
                descriptionHyperlinks = null,
                ignoreConfigErrors = false,
                ignoreIllegalLabelCharacterConfigError = false,
                modExtensions = null,
                shortHash = 0,
                index = 0,
                modContentPack = null,
                fileName = null,
                generated = false,
                debugRandomId = 0,
                labelShort = null,
                valueUnit = null,
                valueColor = default,
                sharesCapacity = false,
                viscosity = 0,
                capacityFactor = 0,
                networkDef = null,
                specialDroppedContainerDef = null,
                thingDroppedFromContainer = null,
                valueToThingRatio = 0
            },
        };
        
        public class NetworkClassTest : IContainerImplementer<NetworkValueDef, IContainerHolderNetwork, NetworkContainer>, IContainerHolderNetwork
        {
            public NetworkClassTest()
            {
                NetworkPart = null;
            }
            
            public string ContainerTitle { get; }
            public void Notify_ContainerStateChanged(NotifyContainerChangedArgs<NetworkValueDef> args)
            {
                //Console.WriteLine($"Funny result args:\n{args}");
            }

            public Thing Thing { get; }

            public bool ShowStorageGizmo { get; }
            public INetworkSubPart NetworkPart { get; }
            public NetworkContainerSet ContainerSet { get; }
            public NetworkContainer Container { get; }
            public NetworkDef NetworkDef { get; }
        }
        
        public NetworkContainer TestContainer { get; set; }
        
        [SetUp]
        public void ContainerInitTest()
        {
            Console.WriteLine("------------------------------------------------------------\n");
            
            TestContainer = new NetworkContainer(new ContainerConfig
            {
                containerClass = null,
                baseCapacity = 10000,
                containerLabel = null,
                storeEvenly = false,
                dropContents = false,
                leaveContainer = false,
                valueDefs = new List<FlowValueDef>(ValueDefs),
                explosionProps = null
            }, new NetworkClassTest());
        }

        [Test]
        public void First()
        {
            int i = 1 + 1;
            Console.Write(i);
        }

        [Test]
        public void BasicContainerFunctionTest()
        {
            var result = TestContainer.TryAddValue((ValueDefs[0], 10));
            Assert.True(result);
            Assert.AreEqual(result.ActualAmount, 10);
        }
        
        [Test]
        public void ProfileTest1()
        {
            var values = new List<DefFloat<NetworkValueDef>>();
            for (int i = 0; i < 1000; i++)
            {
                values.Add(new DefFloat<NetworkValueDef>(ValueDefs[0], 1+i));
            }
            
            Console.WriteLine("Running Arithmetic Profiling");
            
            //PROFILING
            var stopwatch = new Stopwatch();
            
            //PROFILE CONTAINER ADDITION
            Console.WriteLine($"Testing {values.Count} ValueContainer Additions");
            stopwatch.Start();
            var failCount = 0;
            foreach (var value in values)
            {
                var result = TestContainer.TryAddValue(value);
                if (result.State == ValueState.Failed)
                    failCount++;
            }
            stopwatch.Stop();
            Console.WriteLine($"Execution time: {stopwatch.Elapsed} with {failCount} failures to add");
            Console.WriteLine("------------------------------------------------------------\n");
            
            //
            Console.WriteLine($"Testing {values.Count} DefValueStack<> Additions - Immutability & Performance");
            stopwatch.Restart();
            var initStack = new DefValueStack<NetworkValueDef>(ValueDefs);
            initStack += new DefFloat<NetworkValueDef>(ValueDefs[0], 1337);
            var localStack = new DefValueStack<NetworkValueDef>(ValueDefs);
            localStack += new DefFloat<NetworkValueDef>(ValueDefs[0], 1);
            for (int i = 0; i < values.Count; i++)
            {
                localStack += localStack;
            }
            var newStack = initStack + localStack;
            stopwatch.Stop();
            Assert.IsTrue(initStack != localStack && initStack != newStack);
            Console.WriteLine($"Execution time: {stopwatch.Elapsed} | Immutability: {(initStack != localStack && initStack != newStack)}");
            Console.WriteLine("------------------------------------------------------------\n");
            
            stopwatch.Restart();
            Console.WriteLine($"Testing {values.Count} DefFloat<> Additions - Immutability & Performance");
            DefFloat<NetworkValueDef> valueDef = new DefFloat<NetworkValueDef>(ValueDefs[0], 66);
            var newValueFloat = new DefFloat<NetworkValueDef>(ValueDefs[0], 0);
            foreach (var value in values)
            {
                newValueFloat = valueDef + value.Value;
            }
            stopwatch.Stop();
            Assert.IsTrue(valueDef != newValueFloat);
            Console.WriteLine($"Execution time: {stopwatch.Elapsed} | Immutability: {valueDef != newValueFloat}");
            Console.WriteLine("------------------------------------------------------------\n");
            //
            Console.WriteLine($"[Final]Container value:\n{TestContainer}");

        }
    }*/
}

/* TAE REF
#define IS_TELE_DEBUG

using System;
using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using TAE;
using TAE.Atmosphere.Rooms;
using TAE.AtmosphericFlow;
using TeleCore.FlowCore;
using TeleCore.Network.Data;
using TeleCore.Primitive;
using UnityEngine;

namespace TAE_Tests
{
    [TestFixture]
    public class EqualizationTests
    {
        private static List<AtmosphericVolume> volumes;
        private static List<FlowInterface<int,AtmosphericVolume, AtmosphericValueDef>> interfaces;
        private static Dictionary<AtmosphericVolume, List<FlowInterface<int,AtmosphericVolume, AtmosphericValueDef>>> connections;

        public static AtmosphericValueDef[] defs = new AtmosphericValueDef[2]
        {
            new AtmosphericValueDef
            {
                defName = "GasA",
                label = "gas A",
                labelShort = "a",
                valueUnit = "°",
                valueColor = Color.red,
                viscosity = 1,
                friction = 0
            },
            new AtmosphericValueDef
            {
                defName = "GasB",
                label = "gas B",
                labelShort = "b",
                valueUnit = "°",
                valueColor = Color.blue,
                viscosity = 1,
                friction = 0
            },
        };
        
        [SetUp]
        public void Setup()
        {
            var config = new FlowVolumeConfig<AtmosphericValueDef>
            {
                //allowedValues = new List<AtmosphericValueDef>(),
                capacity = 1000
            };

            volumes = new List<AtmosphericVolume>();
            connections = new Dictionary<AtmosphericVolume, List<FlowInterface<AtmosphericVolume, AtmosphericValueDef>>>();
            volumes.Add(new AtmosphericVolume(config));
            volumes.Add(new AtmosphericVolume(config));
            volumes[0].UpdateVolume(10);
            volumes[1].UpdateVolume(10);

            var atmosInterface = new List<FlowInterface<AtmosphericVolume, AtmosphericValueDef>> {new(volumes[0], volumes[1])};
            connections.Add(volumes[0], atmosInterface);
            connections.Add(volumes[1], atmosInterface);

            interfaces = new List<FlowInterface<AtmosphericVolume, AtmosphericValueDef>>(atmosInterface);
        }
        
        [Test]
        public void Equalization()
        {
            var res1 = volumes[0].TryAdd(defs[0], 250);
            var res2 = volumes[0].TryAdd(defs[1], 250);

            int count = 0;
            do
            {
                Equalize(count);
                count++;
                Console.WriteLine($"[{count}][{interfaces[0].NextFlow}]");

            } 
            while (interfaces[0].NextFlow > 0.00001);
                
            var calc = Math.Abs(500d - (volumes[0].TotalValue + volumes[1].TotalValue));
            Assert.IsTrue(calc < 0.00001d);
        }
        
        private void Equalize(int step)
        {
            //Prepare
            foreach (AtmosphericVolume volume in volumes)
            {
                volume.PrevStack = volume.Stack;
            }
            
            //Update Flow
            foreach (var conn in interfaces)
            {
                double flow = conn.NextFlow;      
                var from = conn.From;
                var to = conn.To;
                flow = AtmosphericSystem.FlowFunc(conn, flow);
                conn.UpdateBasedOnFlow(flow);
                flow = Math.Abs(flow);
                conn.NextFlow = AtmosphericSystem.ClampFunc(connections,from, to, flow);
                conn.Move = AtmosphericSystem.ClampFunc(connections, from, to, flow);
            }
            
            //Upate Content
            foreach (var conn in interfaces)
            {
                DefValueStack<AtmosphericValueDef, double> res = conn.From.RemoveContent(conn.Move);
                conn.To.AddContent(res);
                //Console.WriteLine($"Moved: " + conn.Move + $":\n{res}");
                    
                //TODO: Structify for: _connections[fb][i] = conn;
            }
        }
    }
}
*/