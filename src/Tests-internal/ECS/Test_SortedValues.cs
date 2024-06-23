using Friflo.Engine.ECS.Index;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UseObjectOrCollectionInitializer
// ReSharper disable InconsistentNaming
namespace Internal.ECS {

#pragma warning disable CA1861

public static class Test_SortedValues
{
    [Test]
    public static void Test_SortedValues_Add_Remove()
    {
        var map = new SortedValues<int>();
        
        // --- indexer[]
        map[20] = new IdArray(20, 1);
        AreEqual(1, map.Count);
        AreEqual(new [] { 20 }, map.KeySpan.ToArray());
        
        map[30] = new IdArray(30, 1);
        AreEqual(2, map.Count);
        AreEqual(new [] { 20, 30 }, map.KeySpan.ToArray());
        
        map[10] = new IdArray(10, 1);
        AreEqual(3, map.Count);
        AreEqual(new [] { 10, 20, 30 }, map.KeySpan.ToArray());
        
        AreEqual(10, map[10].start);
        AreEqual(20, map[20].start);
        AreEqual(30, map[30].start);
        
        map[10] = new IdArray(11, 1); // set same key again
        AreEqual(3, map.Count);
        AreEqual(new [] { 10, 20, 30 }, map.KeySpan.ToArray());

        AreEqual(11, map[10].start);
        AreEqual(20, map[20].start);
        AreEqual(30, map[30].start);
        
        // --- Remove()
        map.Remove(10);
        AreEqual(2, map.Count);
        AreEqual(new [] { 20, 30 }, map.KeySpan.ToArray());
        
        AreEqual(20, map[20].start);
        AreEqual(30, map[30].start);
        
        map.Remove(10); // remove same key again
        AreEqual(2, map.Count);
        AreEqual(new [] { 20, 30 }, map.KeySpan.ToArray());
        
        AreEqual(20, map[20].start);
        AreEqual(30, map[30].start);
        
        // --- TryGetValue()
        map.TryGetValue(20, out var ids);
        AreEqual(20, ids.start);
        
        map.TryGetValue(42, out ids);
        AreEqual(new IdArray(), ids);
    }
    
    [Test]
    public static void Test_SortedValues_KeyEnumerator()
    {
        var map = new SortedValues<int>();
        map[10] = new IdArray(10, 1);
        map[20] = new IdArray(10, 1);
        
        var keys = new SortedValuesKeys<int>(map);
        int count = 0;
        foreach (var key in keys) {
            switch (count++) {
                case 0: AreEqual(10, key); break;
                case 1: AreEqual(20, key); break;
            }    
        }
        AreEqual(2, count);
        
        map[30] = new IdArray(30, 1);
        AreEqual(3, keys.Count);
    }
}

}
