// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
   
    // TODO add array to add and get custom types by generic type T
    public struct Tick
    {
        public float deltaTime;

        public override string ToString() => $"deltaTime: {deltaTime}";
        
        public Tick(float deltaTime) {
            this.deltaTime = deltaTime;
        }
    }
}