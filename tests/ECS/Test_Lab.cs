using Friflo.Fliox.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
namespace Tests.ECS;

public static class Test_Lab
{
    // [Test]
    public static void Test_Ref_Enumerator() {
        var enumerable = new TestEnumerable<Position, Rotation>();
        foreach (var (position, rotation) in enumerable) {
            position.Value.x = 1;
            // position.x = 3;
        }
        // AreEqual(3, enumerable.value1);
    }
}

public struct RefLab<T> where T : struct
{
    internal    T[]     components;
    internal    int     pos;
    public  ref T       Value => ref components[pos];
}

public struct TestEnumerable<T1, T2> where T1 : struct where T2 : struct
{
    internal RefLab<T1>    value1;
    internal T2         value2;
    
    public TestEnumerator<T1, T2> GetEnumerator() => new TestEnumerator<T1, T2>(this);
}

public struct TestEnumerator<T1, T2> where T1 : struct where T2 : struct
{
    private TestEnumerable<T1, T2>  enumerable;
    private int                     pos;
    
    internal  TestEnumerator(TestEnumerable<T1, T2> enumerable)
    {
        this.enumerable = enumerable;
    }
    
    public (RefLab<T1>, T2) Current   => (enumerable.value1, enumerable.value2);
    
    // --- IEnumerator
    public bool MoveNext() {
        if (pos == 0) {
            pos++;
            return true;  
        }
        return false;
    }
}