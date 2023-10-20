// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

namespace Friflo.Fliox.Engine.ECS.Sync;

public interface IEntityStoreSync
{
    bool TryGetDataNode (long pid, out DataNode dataNode);
    void AddDataNode    (DataNode dataNode);
}