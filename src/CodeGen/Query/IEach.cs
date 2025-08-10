using static CodeGen.Gen;

namespace CodeGen.Query;


static partial class QueryGen {
    
    public static string IEach_generator(int count)
    {
        var param = Join(count, n => $"ref T{n} c{n}", ", ");
        
    return $$"""
// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

public interface IEach<T1, T2>
{
    void Execute({{param}});
}

public interface IEachEntity<T1, T2>
{
    void Execute({{param}}, int id);
}
""";
}
}