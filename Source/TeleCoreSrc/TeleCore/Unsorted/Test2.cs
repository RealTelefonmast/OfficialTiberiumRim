using UnityEngine.Assertions;
using Verse;

namespace TeleCore.Unsorted;

[TestFixture]
public class Test2
{
    [Test]
    public void FloatControllerTest1()
    {
        var controller = new FloatControl(10, 1);
        Assert.IsTrue(controller.CurState == FloatControl.FCState.Idle);
        
        controller.Start();
        Assert.IsTrue(controller.CurState == FloatControl.FCState.Accelerating);
        for (int i = 0; i < 60; i++)
        {
            controller.Tick();
        }
        Assert.IsTrue(controller.ReachedPeak);
        
        controller.Stop();
        Assert.IsTrue(controller.CurState == FloatControl.FCState.Decelerating);
    }
    
    [Test]
    public void FloatControllerTest2()
    {
        var controller = new FloatControl(10, 1,  new SimpleCurve()
        {
            new(0, 0),
            new(2, 2),
        });
        
        Assert.IsTrue(controller.CurState == FloatControl.FCState.Idle);
        
        controller.Start();
        Assert.IsTrue(controller.CurState == FloatControl.FCState.Accelerating);
        for (int i = 0; i < 60; i++)
        {
            controller.Tick();
        }
        Assert.IsTrue(controller.ReachedPeak);
        
        controller.Stop();
        Assert.IsTrue(controller.CurState == FloatControl.FCState.Decelerating);
    }
}