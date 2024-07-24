// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

// Hard Rule! file must not have any dependency a to a specific game engine. E.g. Unity, Godot, Monogame, ...


using Friflo.Engine.ECS.Systems;

// ReSharper disable once CheckNamespace
// ReSharper disable ConvertToPrimaryConstructor
namespace Friflo.Engine.ECS;

/// <summary>
/// Specify <see cref="deltaTime"/> and <see cref="time"/> for system execution in <see cref="SystemGroup.Update"/>.
/// </summary>
/// <remarks>
/// In case of Unity:<br/>
/// <c>MonoBehaviour.Update()</c>:      <c>Time.deltaTime</c>,      <c>Time.time</c><br/> 
/// <c>MonoBehaviour.LateUpdate()</c>:  <c>Time.deltaTime</c>,      <c>Time.time</c><br/> 
/// <c>MonoBehaviour.FixedUpdate()</c>: <c>Time.fixedDeltaTime</c>, <c>Time.fixedTime</c><br/> 
/// </remarks>
public readonly struct UpdateTick
{
    /// <summary> The elapsed time since previous <see cref="SystemGroup.Update"/> execution. </summary>
    public readonly float deltaTime;
    
    /// <summary> The time at the beginning of the current frame. </summary>
    public readonly float time;

    public override string ToString() => $"deltaTime: {deltaTime}";
    
    /// <summary>
    /// Create a <see cref="UpdateTick"/> with the given <paramref name="deltaTime"/> and <paramref name="time"/>.
    /// </summary>
    public UpdateTick(float deltaTime, float time) {
        this.deltaTime  = deltaTime;
        this.time       = time;
    }
}
