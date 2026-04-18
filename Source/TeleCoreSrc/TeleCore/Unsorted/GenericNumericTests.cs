using UnityEngine.Assertions;

namespace TeleCore.Unsorted;

[TestFixture]
public class GenericNumericTests
{
    [Test]
    public void ConstantCheck()
    {
        Numeric<int> num = Numeric<int>.Zero;
        Numeric<int> num2 = Numeric<int>.One;
        Numeric<int> num3 = Numeric<int>.NegativeOne;
        
        Assert.AreEqual(num.Value, 0);
        Assert.AreEqual(num2.Value, 1);
        Assert.AreEqual(num3.Value, -1);
        
        Numeric<float> fNum = Numeric<float>.Zero;
        Numeric<float> fNum2 = Numeric<float>.One;
        Numeric<float> fNum3 = Numeric<float>.NegativeOne;
        Numeric<float> fNum4 = Numeric<float>.Epsilon;
        
        Assert.AreEqual(fNum.Value, 0f);
        Assert.AreEqual(fNum2.Value, 1f);
        Assert.AreEqual(fNum3.Value, -1f);
        Assert.AreEqual(fNum4.Value, float.Epsilon);

        Numeric<double> dNum = Numeric<double>.Zero;
        Numeric<double> dNum2 = Numeric<double>.One;
        Numeric<double> dNum3 = Numeric<double>.NegativeOne;
        Numeric<double> dNum4 = Numeric<double>.Epsilon;
        
        Assert.AreEqual(dNum.Value, 0d);
        Assert.AreEqual(dNum2.Value, 1d);
        Assert.AreEqual(dNum3.Value, -1d);
        Assert.AreEqual(dNum4.Value, double.Epsilon);
    }

    [Test]
    public void RoundTest()
    {
        Numeric<int> num = 10;
        Numeric<float> num2 = 5.4999f;
        Numeric<double> num3 = 5.4999;

        var numRound = MathG.Round(num, 1);
        var num2Round = MathG.Round(num2, 2);
        var num3Round = MathG.Round(num3, 2);

        Assert.AreEqual(numRound, 10);
        Assert.AreEqual(num2Round, System.Math.Round(5.4999f, 2));
        Assert.AreEqual(num3Round, System.Math.Round(5.4999, 2));
    }

    [TestCase(0, 1, ExpectedResult = 0)]
    [TestCase(0.1f, 1.9f, ExpectedResult = 0.1f)]
    public float MinMaxTest_Float(float num1, float num2)
    {
        Assert.AreEqual(num1, MathG.Min(num1, num2));
        Assert.AreEqual(num2, MathG.Max(num1, num2));
        return num1;
    }

    [TestCase(0, 1, ExpectedResult = 0)]
    [TestCase(0.01, 1.99, ExpectedResult = 0.01)]
    public double MinMaxTest_Double(double num1, double num2)
    {
        Assert.AreEqual(num1, MathG.Min(num1, num2));
        Assert.AreEqual(num2, MathG.Max(num1, num2));
        return num1;
    }

    [TestCase(0, 1, ExpectedResult = 0)]
    public int MinMaxTest_int(int num1, int num2)
    {
        Assert.AreEqual(num1, MathG.Min(num1, num2));
        Assert.AreEqual(num2, MathG.Max(num1, num2));
        return num1;
    }
    
}