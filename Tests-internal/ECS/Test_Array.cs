// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_Array
    {
        [Test]
        public static void Test_Array_basics()
        {
            var array = new Array<object>(Array.Empty<object>());
            var object1 = new object();
            var object2 = new object();
            var object3 = new object();
            array.Add(object1);
            array.Add(object2);
            array.Add(object3);
            AreEqual(3, array.Count);
            AreEqual(3, array.Span.Length);
            AreEqual("Object[3]", array.ToString());
            AreSame(object1, array[0]);
            AreSame(object2, array[1]);
            AreSame(object3, array[2]);
            
            var object4 = new object();
            array.InsertAt(1, object4);
            AreSame(object4, array[1]);
            AreEqual(4, array.Count);
            
            AreEqual(1,  array.Remove(object4));
            AreEqual(-1, array.Remove(object4));
            AreEqual(3, array.Count);
            
            //
            var array2= new Array<object>(Array.Empty<object>());
            array2.InsertAt(0, object1); // cover resize
        }
        
        [Test]
        public static void Test_Array_DebugView()
        {
            var array = new Array<object>(Array.Empty<object>());
            var object1 = new object();
            var object2 = new object();
            array.Add(object1);
            array.Add(object2);
            var debugView = new ArrayDebugView<object>(array);
            
            
            AreEqual(2, array.Count);
            AreSame(object1, debugView.Items[0]);
            AreSame(object2, debugView.Items[1]);
        }
        
    }
}