using TeleCore.Types.Structs;
using UnityEngine.Assertions;

namespace TeleCore.Types;

[TestFixture]
public class HashTests
{
    [Test]
    public void TwoWayKeyTest()
    {
        var key = new TwoWayKey<string>("Hewo", "Wowd");
        var key2 = new TwoWayKey<string>("Wowd", "Hewo");

        var equals = key.Equals(key2);

        Assert.AreEqual(key, key2);
    }

    [Test]
    public void IOConnectionTest()
    {
        var ioConn = new IOConnection();
    }
}