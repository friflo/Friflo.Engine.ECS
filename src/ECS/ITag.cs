// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


namespace Friflo.Engine.ECS;

/// <summary>
/// Used to create entity <b>Tag</b>'s by declaring a struct without fields or properties extending <see cref="ITag"/>
/// </summary>
/// <remarks>
/// Optionally attribute the implementing struct with <see cref="TagNameAttribute"/><br/>
/// to assign a custom tag name used for JSON serialization.
/// </remarks>
public interface ITag { }