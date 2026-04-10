using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using TeleCore.Network.IO;
using TeleCore.Utils;
using UnityEngine.Assertions;
using Verse;
using NetworkIO = TeleCore.Network.IO.NetworkIO;

namespace TeleCore.Tests;

[TestFixture]
public class IOTests
{
    [Test]
    public void PatternTest()
    {
        var config = new NetIOConfig()
        {
            patternSize = new IntVec2(1,1),
            pattern = "X",
        };

        var config2 = new NetIOConfig()
        {
            patternSize = new IntVec2(3,3),
            pattern =
                "#X#" +
                "X#X" +
                "#X#",
        };
        
        
        // Assert.Catch(delegate
        // {
        //     config.PostLoad();
        // });
        
        // Assert.Catch(delegate
        // {
        //     config.PostLoad();
        //     var io1 = new NetworkIO(config, new IntVec3(5, 0, 5), Rot4.North);
        // });
        
        config2.PostLoadCustom(null);
        var io2 = new NetworkIO(config2, new IntVec3(5, 0, 5), Rot4.North);
        Assert.IsTrue(io2.IOModeAt(new IntVec3(5,0,6)) == NetworkIOMode.TwoWay);
        Assert.IsTrue(io2.IOModeAt(new IntVec3(4,0,5)) == NetworkIOMode.TwoWay);
        Assert.IsTrue(io2.IOModeAt(new IntVec3(6,0,5)) == NetworkIOMode.TwoWay);
        Assert.IsTrue(io2.IOModeAt(new IntVec3(5,0,4)) == NetworkIOMode.TwoWay);
    }

    [Test]
    public void ConnectionTest()
    {
        var config = new NetIOConfig()
        {
            cellsNorth = new List<IOCellPrototype>()
            {
                new ()
                {
                    direction = Rot4.North,
                    mode = NetworkIOMode.TwoWay,
                    offset = Rot4.North.FacingCell,
                },
                new ()
                {
                    direction = Rot4.South,
                    mode = NetworkIOMode.TwoWay,
                    offset = Rot4.South.FacingCell,
                },
                new ()
                {
                    direction = Rot4.East,
                    mode = NetworkIOMode.TwoWay,
                    offset = Rot4.East.FacingCell,
                },
                new ()
                {
                    direction = Rot4.West,
                    mode = NetworkIOMode.TwoWay,
                    offset = Rot4.West.FacingCell,
                }
            }
        };
        
        config.PostLoadCustom(null);
        
        //5
        //#  +
        //# +3+
        //#  +
        //#   
        //0####5####0####5####0

        NetworkIO io1 = new NetworkIO(config, new IntVec3(3, 0, 3), Rot4.North);
        NetworkIO io2 = new NetworkIO(config, new IntVec3(4, 0, 3), Rot4.North);
        
        //Assert.IsTrue(io1.(io2));
    }

    [Test]
    public void DefaultIOGenerationTest()
    {
        var pattern = IOUtils.DefaultFallbackIfNecessary(null, new IntVec2(3,3));
        var pattern2 = IOUtils.DefaultFallbackIfNecessary(null, new IntVec2(4,4));
        Assert.AreEqual("#X#X+X#X#", pattern);
        Assert.AreEqual("#XX#" +
                        "X++X" +
                        "X++X" +
                        "#XX#", pattern2);
    }

    [Test]
    public void IOPatternTest()
    {
        var pattern = "##X# #| \n## +## | \nX+++X|\n  ##+##|\n  ##X##";
        var patternSize = new IntVec2(5,5);
        pattern =  Regex.Replace(pattern, @"\s+", "");
        
        Console.WriteLine("Pattern: \n" + pattern + "\n");
        Assert.AreEqual(pattern,"##X##|##+##|X+++X|##+##|##X##");
        
        var cells = IOUtils.GenerateFromPattern(pattern, patternSize);
        DrawCells(cells, patternSize - new IntVec2(2,2));
        
        Assert.AreEqual(9, cells.Count );
    }

    public static void DrawCells(List<IOCellPrototype> cells, IntVec2 size)
    {
        string text = "";

        for (int x = 0; x < size.x+2; x++)
        {
            for (int y = 0; y < size.z+2; y++)
            {
                text += "#";
            }
        }
        var center = new IntVec3(System.Math.Abs(0 - (size.x+2 - 1) / 2),0, System.Math.Abs(0 - (size.z+2 - 1) / 2));
        foreach (var cell in cells)
        {
            var pos = center + cell.offset;
            var index = pos.z * (size.x+2) + pos.x;
            var arr = text.ToCharArray();
            arr[index] = ForMode(cell.mode);
            text = new string(arr);
        }
        
        Console.WriteLine("Cells: \n" + text + "\n");
    }

    public static char ForMode(NetworkIOMode mode)
    {
        return mode switch
        {
            NetworkIOMode.TwoWay => 'X',
            NetworkIOMode.Input => 'I',
            NetworkIOMode.Output => 'O',
            NetworkIOMode.Visual => '+',
            _ => '#'
        };
    }
    
    [Test]
    public void IOCellGenerationTest()
    {
        var cells = IOUtils.GenerateFromPattern(null, new IntVec2(3, 3));
        var config = new NetIOConfig()
        {
            cellsNorth = cells,
        };

        var result = new List<IOCellPrototype>();
       //NetIOConfig.Rotate(cells, ref result);
        
        config.PostLoadCustom(null);

        var netIO = new NetworkIO(config, new IntVec3(5, 0, 5), Rot4.North);
        Assert.AreEqual(4, netIO.Connections.Count);
        Assert.AreEqual(1, netIO.VisualCells.Count);
    }
}