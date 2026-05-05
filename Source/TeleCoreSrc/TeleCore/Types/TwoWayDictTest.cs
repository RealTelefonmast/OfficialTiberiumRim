using UnityEngine.Assertions;

namespace TeleCore.Types;

//[TestFixture]
public class TwoWayDictTest
{
    //[Test]
    public void TestShared()
    {
        var first = (object)"Woah man!";
        var second = (object)"Pepega :D";
        var third = (object)"wazzuuupppp";

        TwoWayDictionary<object, double> dict = new();
        dict.TryAdd(first, 69);
        dict.TryAdd(second, 1337);
        dict.TryAdd(third, 69);

        Assert.AreEqual(dict.Count, 3);
        Assert.AreEqual(dict["Woah man!"], 69);
        Assert.AreEqual(dict["Pepega :D"], 1337);
        Assert.AreEqual(dict["wazzuuupppp"], 69);
        Assert.AreEqual(dict[69d].Count, 2);
    }
}