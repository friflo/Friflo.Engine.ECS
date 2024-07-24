using System;
using Friflo.Engine.ECS;
using NUnit.Framework;

namespace Internal.ECS {

// ReSharper disable InconsistentNaming
public static class Test_StackArray
{
    [Test]
    public static void Test_StackArray_container()
    {
        var stack = new StackArray<int>(Array.Empty<int>());
        
        for (int n = 0; n < 100; n++) {
            Assert.AreEqual(n, stack.Count);
            stack.Push(n);
        }
        Assert.AreEqual(100, stack.Count);
        Assert.AreEqual("Count: 100", stack.ToString());
        
        int value;
        for (int n = 99; n >= 0; n--) {
            Assert.IsTrue(stack.TryPop(out value));
            Assert.AreEqual(n, value);
            Assert.AreEqual(n, stack.Count);
        }
        Assert.IsFalse(stack.TryPop(out value));
        Assert.AreEqual(0, stack.Count);
        
        stack.Push(42);
        Assert.AreEqual(1, stack.Count);
        
        stack.Clear();
        Assert.AreEqual(0, stack.Count);
    }
}

}