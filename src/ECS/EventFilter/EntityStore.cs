// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;


public partial class EntityStore
{
    private EventRecorder GetEventRecorder() {
        if (intern.eventRecorder != null) {
            return intern.eventRecorder;
        }
        return intern.eventRecorder = new EventRecorder(this);
    }
}