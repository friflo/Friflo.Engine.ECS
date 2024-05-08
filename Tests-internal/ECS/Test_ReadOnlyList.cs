// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Collections.Generic;
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
        public static void Test_Array_mutate()
        {
            var array = new ReadOnlyList<object>(Array.Empty<object>());
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
            
            array.RemoveAt(1);
            AreEqual(2,         array.Count);
            AreSame(object1,    array[0]);
            AreSame(object3,    array[1]);
            AreEqual(1,         array.IndexOf(object3));
            AreEqual(-1,        array.IndexOf(object4));
            //
            var array2= new ReadOnlyList<object>(Array.Empty<object>());
            array2.InsertAt(0, object1); // cover resize
        }
        
        [Test]
        public static void Test_Array_enumerator()
        {
            var array = new ReadOnlyList<object>(Array.Empty<object>());
            var object1 = new object();
            array.Add(object1);
            {
                IEnumerable enumerable = array;
                IEnumerator enumerator = enumerable.GetEnumerator();
                using var enumerator1 = enumerator as IDisposable;
                int count = 0;
                while (enumerator.MoveNext()) {
                    count++;
                }
                AreEqual(1, count);
                
                count = 0;
                enumerator.Reset();
                while (enumerator.MoveNext()) {
                    count++;
                    AreSame(object1, enumerator.Current);
                }
                AreEqual(1, count);
            }
            {
                IEnumerable<object> enumerable = array;
                using var enumerator = enumerable.GetEnumerator();
                int count = 0;
                while (enumerator.MoveNext()) {
                    count++;
                }
                AreEqual(1, count);
            }
        }
        
        
        [Test]
        public static void Test_Array_DebugView()
        {
            var array = new ReadOnlyList<object>(Array.Empty<object>());
            var object1 = new object();
            var object2 = new object();
            array.Add(object1);
            array.Add(object2);
            var debugView = new ReadOnlyListDebugView<object>(array);
            
            
            AreEqual(2, array.Count);
            AreSame(object1, debugView.Items[0]);
            AreSame(object2, debugView.Items[1]);
        }
        
    }
}