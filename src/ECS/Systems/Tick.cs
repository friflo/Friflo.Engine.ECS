// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
namespace Friflo.Engine.ECS
{
    // May add an array or field to add and get custom types (a class) by generic type T
    public readonly struct Tick
    {
        public readonly float deltaTime;

        public override string ToString() => $"deltaTime: {deltaTime}";
        
        public static implicit operator Tick(float deltaTime) => new Tick(deltaTime);
        
        public Tick(float deltaTime) {
            this.deltaTime = deltaTime;
        }
    }
}