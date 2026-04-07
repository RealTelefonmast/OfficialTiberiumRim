namespace TeleCore;

//[TestFixture]
public class RelativeDirTests
{
    /*[Test]
    public void RelativeDirTest()
    {
        IntVec3 beTop = new IntVec3(0, 0, 2);
        IntVec3 beRight = new IntVec3(2, 0, 0);
        IntVec3 beLeft = new IntVec3(-2, 0, 0);
        IntVec3 beBottom = new IntVec3(0, 0, -2);
        IntVec2 size = new IntVec2(2,2);
        var dirT = IOUtils.RelativeDir(IntVec3.Zero, beTop, size);
        var dirR = IOUtils.RelativeDir(IntVec3.Zero, beRight, size);
        var dirL = IOUtils.RelativeDir(IntVec3.Zero, beLeft, size);
        var dirB = IOUtils.RelativeDir(IntVec3.Zero, beBottom, size);
        
        Assert.AreEqual(Rot4.North, dirB);
        Assert.AreEqual(Rot4.East, dirR);
        Assert.AreEqual(Rot4.West, dirL);
        Assert.AreEqual(Rot4.South, dirT);
        
    }

    [Test]
    public void CellRectTest()
    {
        int width = 4;
        int height = 4;
        
        var rect = new CellRect(0, 0, width / 2, height / 2);
        var pat = $"\n{rect.Width}|{rect.Height}\n";
        int i = 0;
        foreach (var cell in rect.Cells)
        {
            if(i % rect.Width == 0)
                pat += "\n";
            
            if (cell == IntVec3.Zero)
            {
                pat += "Z";
                i++;
                continue;
            }
            
            if (cell == rect.CenterCell)
            {
                pat += "C";
                i++;
                continue;
            }   
            
            var dir = IOUtils.RelativeDir(rect, cell);
            if (dir.IsValid)
            {
                if (dir == Rot4.North)
                {
                    pat += "N";
                }
                else if (dir == Rot4.East)
                {
                    pat += "E";
                }
                else if (dir == Rot4.West)
                {
                    pat += "W";
                }
                else if (dir == Rot4.South)
                {
                    pat += "S";
                }
                i++;
                continue;
            }
            
            pat += "#";
            i++;
        }
        
        rect = rect.ExpandedBy(1);
        var pat2 = $"\n{rect.Width}|{rect.Height}\n";
        i = 0;
        foreach (var cell in rect.Cells)
        {
            if(i % rect.Width == 0)
                pat2 += "\n";
            
            if (cell == IntVec3.Zero)
            {
                pat2 += "Z";
                i++;
                continue;
            }
            
            if (cell == rect.CenterCell)
            {
                pat2 += "C";
                i++;
                continue;
            }

            var dir = IOUtils.RelativeDir(IntVec3.Zero, cell, new IntVec2(rect.Width/2, rect.Height/2));
            if (dir.IsValid)
            {
                if (dir == Rot4.North)
                {
                    pat2 += "N";
                }
                else if (dir == Rot4.East)
                {
                    pat2 += "E";
                }
                else if (dir == Rot4.West)
                {
                    pat2 += "W";
                }
                else if (dir == Rot4.South)
                {
                    pat2 += "S";
                }
                i++;
                continue;
            }
            
            pat2 += "#";
            i++;
        }
        
        Console.WriteLine(pat);
        Console.WriteLine(pat2);

        Assert.True(true);
    }*/
}