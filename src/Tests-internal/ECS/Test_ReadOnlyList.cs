// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Internal.ECS {

    // ReSharper disable once InconsistentNaming
    public static class Test_Array
    {
        [Test]
        public static void Test_Array_mutate()
        {
            var list = new ReadOnlyList<object>(Array.Empty<object>());
            var object1 = new object();
            var object2 = new object();
            var object3 = new object();
            list.Add(object1);
            list.Add(object2);
            list.Add(object3);
            AreEqual(3, list.Count);
            AreEqual(3, list.Span.Length);
            AreEqual("Object[3]", list.ToString());
            AreSame(object1, list[0]);
            AreSame(object2, list[1]);
            AreSame(object3, list[2]);
            
            var object4 = new object();
            list.Insert(1, object4);
            AreSame(object4, list[1]);
            AreEqual(4,     list.Count);
            
            AreEqual(1,     list.Remove(object4));
            AreEqual(-1,    list.Remove(object4));
            AreEqual(3,     list.Count);
            IsNull  (       list.array[3]);
            
            list.RemoveAt(1);
            AreEqual(2,         list.Count);
            IsNull  (           list.array[2]);
            AreSame (object1,   list[0]);
            AreSame (object3,   list[1]);
            AreEqual(1,         list.IndexOf(object3));
            AreEqual(-1,        list.IndexOf(object4));
            //
            var list2= new ReadOnlyList<object>(Array.Empty<object>());
            list2.Insert(0, object1); // cover resize
        }
        
        [Test]
        public static void Test_Array_enumerator()
        {
            var list = new ReadOnlyList<object>(Array.Empty<object>());
            var object1 = new object();
            list.Add(object1);
            {
                IEnumerable enumerable = list;
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
                IEnumerable<object> enumerable = list;
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
            var list = new ReadOnlyList<object>(Array.Empty<object>());
            var object1 = new object();
            var object2 = new object();
            list.Add(object1);
            list.Add(object2);
            var debugView = new ReadOnlyListDebugView<object>(list);
            
            
            AreEqual(2, list.Count);
            AreSame(object1, debugView.Items[0]);
            AreSame(object2, debugView.Items[1]);
        }
        
    }
}