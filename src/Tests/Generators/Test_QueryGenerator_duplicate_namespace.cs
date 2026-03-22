using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable InconsistentNaming
// ReSharper disable CheckNamespace
namespace Tests.Generators_DuplicateNamespace;


/// Test using same class / method name as in <see cref="Tests.Generators.MyExample.MoveExample"/>
/// Goal: Ensure the source generator uses the namespace in the filename. 
public partial class MyExample
{
    [Query]
    void MoveExample(ref Position position, Entity entity) {
        AreEqual(1, entity.Id); 
        position.x = 1;
    }
}