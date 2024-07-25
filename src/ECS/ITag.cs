// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable RedundantTypeDeclarationBody
namespace Friflo.Engine.ECS;

/// <summary>
/// Used to create entity <b>Tag</b>'s by declaring a struct without fields or properties extending <see cref="ITag"/>.<br/>
/// <b>Note:</b> An <see cref="ITag"/> should be used to tag a group of multiple entities.<br/>
/// See <a href="https://friflo.gitbook.io/friflo.engine.ecs/examples/general#tag">Example.</a>
/// </summary>
/// <remarks>
/// In case you want to find a unique entity add the component <see cref="UniqueEntity"/> to an entity<br/>
/// and use <see cref="EntityStoreBase.GetUniqueEntity"/> to query for this entity.<br/>
/// <br/>
/// Optionally attribute the implementing struct with <see cref="TagNameAttribute"/><br/>
/// to assign a custom tag name used for JSON serialization.
/// </remarks>
public interface ITag { }


/// <summary>
/// If entity <see cref="Entity.Enabled"/> == false it is tagged with <see cref="Disabled"/>.<br/>
/// Disabled entities are excluded from query results by default. To include use <see cref="ArchetypeQuery.WithDisabled"/>.
/// </summary>
[ComponentSymbol("D", "150,150,150")]
public struct Disabled : ITag { };