// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


using System.Collections;
using System.Collections.Generic;

// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Index;

internal readonly struct Relations<TComponent> : IEnumerable<TComponent>
    where TComponent : struct, IComponent
{
    
    public   readonly   int             Length;     //  4
    internal readonly   int             start;      //  4
    internal readonly   int[]           positions;  //  8
    internal readonly   TComponent[]    components; //  8
    internal readonly   int             position;   //  4
    
    internal Relations(TComponent[] components, int[] positions, int start, int length)
    {
        this.components = components;
        this.positions  = positions;
        this.start      = start;
        Length          = length;
    }
   
    internal Relations(TComponent[] components, int position) {
        this.components = components;
        this.position   = position;
        Length          = 1;
    }
    
    public TComponent this[int index] => components[positions != null ? positions[index] : position];
       
    // --- IEnumerable<>
    IEnumerator<TComponent>   IEnumerable<TComponent>.GetEnumerator() => new RelationsEnumerator<TComponent>(this);
    
    // --- IEnumerable
    IEnumerator                           IEnumerable.GetEnumerator() => new RelationsEnumerator<TComponent>(this);
    
    // --- new
    internal RelationsEnumerator<TComponent>          GetEnumerator() => new RelationsEnumerator<TComponent>(this);
}


internal struct RelationsEnumerator<TComponent> : IEnumerator<TComponent>
    where TComponent : struct, IComponent
{
    private  readonly   int[]           positions;
    private  readonly   int             position;
    private  readonly   TComponent[]    components;
    private  readonly   int             start;
    private  readonly   int             last;
    private             int             index;
    
    
    internal RelationsEnumerator(Relations<TComponent> relations) {
        positions   = relations.positions;
        position    = relations.position;
        components  = relations.components;
        start       = relations.start;
        last        = start + relations.Length;
        index       = start;
    }
    
    // --- IEnumerator<>
    public readonly TComponent Current   => components[positions != null ? positions[index] : position];
    
    // --- IEnumerator
    public bool MoveNext() {
        if (index < last) {
            index++;
            return true;
        }
        return false;
    }

    public void Reset() {
        index = start;
    }
    
    object IEnumerator.Current => components[positions != null ? positions[index] : position];

    // --- IDisposable
    public void Dispose() { }
}