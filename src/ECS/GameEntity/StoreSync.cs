// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Friflo.Fliox.Engine.Client;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;


// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Engine.ECS;

public interface IEntityStoreSync
{
    bool TryGetDataNode (long pid, out DataNode dataNode);
    void AddDataNode    (DataNode dataNode);
}