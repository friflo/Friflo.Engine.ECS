// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Internal.ECS {

    // ReSharper disable once InconsistentNaming
    public static class Test_Array
    {
        [Test]
        public static void Test_Array_DebugView()
        {
            var list = new ReadOnlyList<object>.Mutate(Array.Empty<object>());
            var object1 = new object();
            var object2 = new object();
            list.Add(object1);
            list.Add(object2);
            var debugView = new ReadOnlyListDebugView<object>(list.list);
            
            
            AreEqual(2, list.Count);
            AreSame(object1, debugView.Items[0]);
            AreSame(object2, debugView.Items[1]);
        }
        
    }
}