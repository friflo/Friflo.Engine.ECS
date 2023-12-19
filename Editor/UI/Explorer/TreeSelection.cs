// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Engine.ECS.Collections;

namespace Friflo.Editor.UI.Explorer;

public readonly struct TreeSelection
{
    internal            int             Length => items.Length;
    
    internal readonly   ExplorerItem[]  items;
    
    internal TreeSelection(ExplorerItem[] items) {
        this.items  = items;
    }
}