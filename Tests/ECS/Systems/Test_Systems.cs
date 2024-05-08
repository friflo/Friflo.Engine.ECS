// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Json.Fliox.Mapper;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable LoopCanBeConvertedToQuery
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Systems
{
    // ReSharper disable once InconsistentNaming
    public static class Test_Systems
    {
        // [Test]
        public static void Test_Systems_serialize()
        {
            var typeStore = new TypeStore();
            var positionMapper = typeStore.GetTypeMapper<PositionSystem>();
            var mapper = new ObjectMapper();
            
            var positionSystem = new PositionSystem { x = 2 };
            var json = mapper.Write(positionSystem);
            AreEqual("", json);
        }
    }
}