// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Runtime.CompilerServices;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


internal static class StructPadding<T>
    where T : struct
{
      private static int GetByteSize() {
        // Unity: when testing as dll in Assets/Plugins folder add required dll's
        //  Friflo.Json.Fliox.Hub.dll
        //  Friflo.Json.Fliox.dll
        //  Friflo.Json.Fliox.Annotation.dll
        //  Friflo.Json.Burst.dll
        //  Friflo.Engine.Hub.dll
        //  Friflo.Engine.ECS.dll
        //  System.Runtime.CompilerServices.Unsafe.dll
        // 
        //  System.Runtime.CompilerServices.Unsafe.dll can be downloaded from
        //      https://www.nuget.org/packages/System.Runtime.CompilerServices.Unsafe/6.0.0
        return Unsafe.SizeOf<T>();
    }

    // ReSharper disable StaticMemberInGenericType
    internal static readonly    int ByteSize        = GetByteSize();

    /// <summary>
    /// The returned padding enables using Vector128, Vector256 and Vector512 (512 bits = 64 bytes) operations <br/>
    /// on <see cref="StructHeap{T}"/>.<see cref="StructHeap{T}.components"/>
    /// without the need of an additional for loop to process the elements at the end of a <see cref="Span{T}"/>.
    /// </summary>
    internal static readonly    int PadCount512     = 64 / ByteSize - 1;
    
    /// <summary> 256 bits = 32 bytes </summary>
    internal static readonly    int PadCount256     = 32 / ByteSize - 1;
    
    /// <summary> 128 bits = 16 bytes </summary>
    internal static readonly    int PadCount128     = 16 / ByteSize - 1;
    
    /// <summary>
    /// Return the number of components in a <see cref="Chunk{T}"/> as a multiple of 64 bytes.
    /// </summary>
    /// <remarks>
    /// This enables providing <see cref="Chunk{T}"/> components as <see cref="Span{T}"/> of Vector128, Vector256 and Vector512
    /// of https://learn.microsoft.com/en-us/dotnet/api/system.runtime.intrinsics.<br/>
    /// See: <see cref="Chunk{T}.AsSpan128{TTo}"/>, <see cref="Chunk{T}.AsSpan256{TTo}"/> and <see cref="Chunk{T}.AsSpan512{TTo}"/>.<br/>
    /// <br/>
    /// It also enables to apply vectorization without a remainder loop.<br/>
    /// </remarks>
    internal static readonly    int ComponentMultiple = GetComponentMultiple();
    
    private static int GetComponentMultiple()
    {
        var lcm = QueryJob.LeastCommonMultiple(ByteSize, 64) / ByteSize;
        if (lcm <= ArchetypeUtils.MaxComponentMultiple) {
            return lcm;
        }
        return 0;
    }
}
