using UnityEngine.Assertions;

namespace TeleCore.Unsorted;

[TestFixture]
public class ImmutableArrayTests
{
    public static LightImmutableArray<int> _staticDefault;
    
    [Test]
    public void DefaultTest()
    {
        Assert.NotNull(_staticDefault.Array);
        Assert.AreEqual(_staticDefault.Length, 0);
    }
    
    [Test]
    public void InstanceTests()
    {
        var instance = new LightImmutableArray<int>(69);
        Assert.NotNull(instance.Array);
        Assert.AreEqual(instance.Length, 1);
        Assert.AreEqual(instance.Array, new int[] {69});
        Assert.AreEqual(instance[0], 69);
    }
}