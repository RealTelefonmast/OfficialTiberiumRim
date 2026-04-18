using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TMPro;
using Verse;

namespace TeleCore.Unsorted;

public static class IOUtils
{
    //  \\\\\\\\
    //  \\\XX\\\
    //  \\#++#\\
    //  \I++++O\
    //  \I++++O\
    //  \\#++#\\
    //  \\\XX\\\
    //  \\\\\\\\

    public const char Input = 'I';
    public const char Output = 'O';
    public const char TwoWay = 'X';
    public const char Empty = '#';
    public const char Visual = '+';
    public const char Logical = 'L';

    internal const string RegexPattern = @"\[[^\]]*\]|.";

    public static NetworkIOMode ModeFromChar(char c)
    {
        return c switch
        {
            Input => NetworkIOMode.Input,
            Output => NetworkIOMode.Output,
            TwoWay => NetworkIOMode.TwoWay,
            Empty => NetworkIOMode.None,
            Visual => NetworkIOMode.Visual,
            Logical => NetworkIOMode.Logical,
            _ => throw new ArgumentException($"Invalid IO mode character: {c}")
        };
    }
    
    public static char CharFromMode(NetworkIOMode mode)
    {
        return mode switch
        {
            NetworkIOMode.Input => Input,
            NetworkIOMode.Output => Output,
            NetworkIOMode.TwoWay => TwoWay,
            NetworkIOMode.None => Empty,
            NetworkIOMode.Visual => Visual,
            NetworkIOMode.Logical => Logical,
            _ => throw new ArgumentException($"Invalid IO mode: {mode}")
        };
    }

    public static bool MatchesIO(this NetworkIOMode innerMode, NetworkIOMode outerMode)
    {
        var innerInput = (innerMode & NetworkIOMode.Input) == NetworkIOMode.Input;
        var outerInput = (outerMode & NetworkIOMode.Input) == NetworkIOMode.Input;

        var innerOutput = (innerMode & NetworkIOMode.Output) == NetworkIOMode.Output;
        var outerOutput = (outerMode & NetworkIOMode.Output) == NetworkIOMode.Output;

        return (innerInput && outerOutput) || (outerInput && innerOutput);
    }
    
    public static List<IOCellPrototype> GenerateFromPattern(string ioPattern, IntVec2 patternSize)
    {
        var size = patternSize;
        var width = size.x - 2;
        var height = size.z - 2;
        var rect = new CellRect(0 - (width - 1) / 2, 0 - (height - 1) / 2, width, height).ExpandedBy(1);
        var rectList = rect.ToArray();
        
        ioPattern = DefaultFallbackIfNecessary(ioPattern, size);
        var modeGrid = GetIOModeArray(ioPattern);
        
        var result = new List<IOCellPrototype>();

        for (var i = 0; i < rect.Area; i++)
        {
            var row = i / rect.Width;
            var column = i % rect.Width;

            var actualIndex = row * rect.Width + column;
            var invertedIndex = (rect.Height - row - 1) * rect.Width + column; //Need inversion of cell getter because of weird indexing shit
            var ioMode = modeGrid[actualIndex];
            var cell = rectList[invertedIndex];

            if (ioMode != NetworkIOMode.None)
            {
                var rel = CellUtils.RelativeDir(rect, cell);
                result.Add(new IOCellPrototype
                {
                    offset = cell,
                    direction = rel,
                    mode = ioMode
                });
            }
        }

        return result;
    }

    /// <summary>
    ///     Rotates the pattern array to match the rotation of the thing.
    /// </summary>
    internal static IOCell[] RotateIOCells(IOCell[] arr, Rot4 rotation, IntVec2 size)
    {
        var xWidth = size.x;
        var yHeight = size.z;
        if (rotation == Rot4.East) arr = arr.RotateLeft(xWidth, yHeight);
        if (rotation == Rot4.South) arr = arr.FLipHorizontal(xWidth, yHeight);
        if (rotation == Rot4.West) arr = arr.RotateRight(xWidth, yHeight);

        return arr;
    }

    public static NetworkIOMode[] GetIOModeArray(string input)
    {
        var matches = Regex.Matches(input, RegexPattern);
        var modeGrid = new NetworkIOMode[matches.Count];
        for (var i = 0; i < matches.Count; i++)
        {
            var match = matches[i];
            if (match.Value.Length == 1)
                modeGrid[i] = ModeFromChar(match.Value[0]);
        }

        return modeGrid;
    }

    public static string DefaultFallbackIfNecessary(string pattern, IntVec2 size)
    {
        if (pattern != null) return pattern;

        var widthx = size.x;
        var heighty = size.z;

        var charArr = new char[widthx * heighty];

        for (var x = 0; x < widthx; x++)
        {
            for (var y = 0; y < heighty; y++)
            {
                //Corners
                if ((x < 1 || x >= widthx-1) && (y < 1 || y >= heighty-1))
                {
                    charArr[x + y * widthx] = Empty;
                    continue;
                }
                
                //Inside
                if ((x >= 1 && x < widthx-1) && (y >= 1 && y < heighty-1))
                {
                    charArr[x + y * widthx] = Visual;
                    continue;
                }
                
                //Sides
                charArr[x + y * widthx] = TwoWay;
            }
        }

        return charArr.ArrayToString();
    }
}