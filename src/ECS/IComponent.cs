// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable RedundantTypeDeclarationBody
namespace Friflo.Engine.ECS;

/// <summary>
/// To enable adding a struct component to an <see cref="Entity"/> it need to implement <see cref="IComponent"/>.<br/>
/// <see cref="IComponent"/> types are <b>struct</b>s which only contains data <b>but no</b> script / methods.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#component">Example.</a>
/// </summary>
/// <remarks>
/// An <see cref="Entity"/> can contain multiple components but only one of each type.<br/>
/// <br/>
/// Optionally attribute the implementing struct with <see cref="ComponentKeyAttribute"/><br/>
/// to assign a custom component key name used for JSON serialization.<br/>
/// <br/>
/// Common game specific <see cref="IComponent"/> types defined by the Engine:
/// <list type="bullet">
///     <item><see cref="EntityName"/></item>
///     <item><see cref="UniqueEntity"/></item>
///     <item><see cref="Position"/></item>
///     <item><see cref="Rotation"/></item>
///     <item><see cref="Scale3"/></item>
///     <item><see cref="Transform"/></item>
/// </list>
/// </remarks>
public interface IComponent { }
