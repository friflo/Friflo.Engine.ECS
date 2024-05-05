// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable once CheckNamespace
namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_SystemMatch
    {
        [Test]
        public static void Test_SystemMatch_GetMatchingSystems()
        {
            var store1  = new EntityStore();
            var store2  = new EntityStore();
            var entity1 = store1.CreateEntity(new Position(1,1,1));
            var entity2 = store2.CreateEntity(new Position(2,2,2), new Scale3(3,3,3));
            
            
            var root    = new SystemRoot(store1, "Systems");
            root.AddStore(store2);
            var group1  = new SystemGroup("Group-1");
            var group2  = new SystemGroup("Group-2");
            root.AddSystem(group1);
            root.AddSystem(group2);
            var positionSystem  = new PositionSystem();
            var scaleSystem     = new ScaleSystem();
            group1.AddSystem(positionSystem);
            group2.AddSystem(scaleSystem);
            
            var matches = new List<SystemMatch>();
            
            // --- add groups
            root.GetMatchingSystems(entity1, matches, true);
            AreEqual(2, matches.Count);
            AreEqual("Group-1 [1] - Depth: 1",      matches[0].ToString());
            AreEqual("PositionSystem - Depth: 2",   matches[1].ToString());
            
            var match0 = matches[0];
            AreSame (group1,        match0.System);
            AreEqual(1,             match0.Count);
            AreEqual(1,             match0.Depth);
            
            var match1 = matches[1];
            AreSame (positionSystem,match1.System);
            AreEqual(1,             match1.Count);
            AreEqual(2,             match1.Depth);
            
            root.GetMatchingSystems(entity2, matches, true);
            AreEqual(4, matches.Count);
            AreEqual("Group-1 [1] - Depth: 1",      matches[0].ToString());
            AreEqual("PositionSystem - Depth: 2",   matches[1].ToString());
            AreEqual("Group-2 [1] - Depth: 1",      matches[2].ToString());
            AreEqual("ScaleSystem - Depth: 2",      matches[3].ToString());
            
            // --- flat
            root.GetMatchingSystems(entity1, matches, false);
            AreEqual(1, matches.Count);
            AreEqual("PositionSystem - Depth: 0",   matches[0].ToString());
            
            root.GetMatchingSystems(entity2, matches, false);
            AreEqual(2, matches.Count);
            AreEqual("PositionSystem - Depth: 0",   matches[0].ToString());
            AreEqual("ScaleSystem - Depth: 0",      matches[1].ToString());
        }
        
        [Test]
        public static void Test_SystemMatch_exceptions()
        {
            var store   = new EntityStore();
            var entity  = store.CreateEntity();
            var root    = new SystemRoot("Systems");
            var matches = new List<SystemMatch>();
            
            var e = Throws<ArgumentNullException>(() => {
                root.GetMatchingSystems(entity, null, true);
            });
            AreEqual("Value cannot be null. (Parameter 'target')", e!.Message);
            
            e = Throws<ArgumentNullException>(() => {
                root.GetMatchingSystems(default, matches, true);
            });
            AreEqual("Value cannot be null. (Parameter 'entity')", e!.Message);
            
            e = Throws<ArgumentNullException>(() => {
                root = null;
                root.GetMatchingSystems(entity, matches, true);
            });
            AreEqual("Value cannot be null. (Parameter 'systemGroup')", e!.Message);
        }
    }
}