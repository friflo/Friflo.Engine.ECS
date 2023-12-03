// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Fliox.Engine.ECS.Collections;

namespace Friflo.Fliox.Editor.UI.Explorer;

public readonly struct TreeSelection
{
    internal            int             Length => items.Length;
    
    internal readonly   ExplorerItem[]  items;
    
    internal TreeSelection(ExplorerItem[] items) {
        this.items  = items;
    }
}