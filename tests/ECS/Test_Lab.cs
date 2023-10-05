using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Lab
{
    [Test]
    public static void Test_Ref_Enumerator() {
        var enumerable = new TestEnumerable<Position>();
        foreach (ref var value in enumerable) {
            value.x = 3;
        }
        AreEqual(3, enumerable.value.x);
    }
}

public class TestEnumerable<T> 
    where T : struct
{
    internal T       value;
    
    public TestEnumerator<T> GetEnumerator() => new TestEnumerator<T>(this);
}

public struct TestEnumerator<T>
    where T : struct
{
    private TestEnumerable<T>   enumerable;
    private int                 pos;
    
    internal  TestEnumerator(TestEnumerable<T> enumerable)
    {
        this.enumerable = enumerable;
    }
    
    public ref T Current   => ref enumerable.value;
    
    // --- IEnumerator
    public bool MoveNext() {
        if (pos == 0) {
            pos++;
            return true;  
        }
        return false;
    }
}