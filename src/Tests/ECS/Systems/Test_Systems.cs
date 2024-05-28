// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Reflection;
using Friflo.Engine.ECS.Systems;
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
        [Test]
        public static void Test_Systems_serialize()
        {
            var typeStore = new TypeStore();
            var positionMapper = typeStore.GetTypeMapper<PositionSystem>();
            NotNull(positionMapper);
            
            var mapper = new ObjectMapper(typeStore);
            {
                var writeSystem = new PositionSystem { x = 42 };
                var json = mapper.Write(writeSystem);
                AreEqual("{\"id\":0,\"enabled\":true,\"x\":42}", json);
                
                var readSystem = mapper.Read<PositionSystem>(json);
                AreEqual(42, readSystem.x);
            } {
                var writeSystem = new SystemGroup("Update");
                var json = mapper.Write(writeSystem);
                AreEqual("{\"id\":0,\"enabled\":true,\"name\":\"Update\"}", json);
                
                var readSystem = mapper.Read<SystemGroup>(json);
                AreEqual("Update", readSystem.Name);
            } {
                var writeSystem = new MySystem1 { value = "test" };
                var json = mapper.Write(writeSystem);
                AreEqual("{\"id\":0,\"enabled\":true,\"value\":\"test\"}", json);
                
                var readSystem = mapper.Read<MySystem1>(json);
                AreEqual("test", readSystem.value);
            } {
                var writeSystem = new SystemRoot("Systems");
                var e = Throws<TargetInvocationException>(() => {
                    mapper.Write(writeSystem);
                });
                var inner = e!.InnerException;
                IsTrue(inner is ArgumentException);
                StringAssert.StartsWith("Type 'Friflo.Engine.ECS.Systems.SystemRoot' does not have a default constructor", inner.Message);
            }
        }
    }
}