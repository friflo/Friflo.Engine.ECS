// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;

namespace Friflo.Fliox.Engine.ECS.Database;

public interface IDatabaseSync
{
    bool                                            TryGetEntity (long pid, out DatabaseEntity databaseEntity);
    void                                            AddEntity    (DatabaseEntity databaseEntity);
    int                                             EntityCount { get; }
    IEnumerable<KeyValuePair<long, DatabaseEntity>> Entities { get; }
}
