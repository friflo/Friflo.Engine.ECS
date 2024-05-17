// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
namespace Friflo.Engine.ECS;


public readonly struct UpdateTick
{
    public readonly float deltaTime;

    public override string ToString() => $"deltaTime: {deltaTime}";
    
    public UpdateTick(float deltaTime) {
        this.deltaTime  = deltaTime;
    }
}
