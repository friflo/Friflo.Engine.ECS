using System.Collections.Generic;
using NUnit.Framework;


// ReSharper disable ConditionalTernaryEqualBranch
// ReSharper disable CompareOfFloatsByEqualityOperator
// ReSharper disable StringLiteralTypo
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Arch {

public static class Test_StructuralChange_Docs
{
[Test]
public static void CollectionModifiedException()
{
    var list = new List<int> { 1, 2, 3 };
    foreach (var item in list) {
        list.Add(42); // throws InvalidOperationException : Collection was modified; enumeration operation may not execute.
    }
}
    
}

}

