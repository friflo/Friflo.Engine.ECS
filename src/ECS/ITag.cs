// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.


// ReSharper disable RedundantTypeDeclarationBody
namespace Friflo.Engine.ECS;

/// <summary>
/// Used to create entity <b>Tag</b>'s by declaring a struct without fields or properties extending <see cref="ITag"/>.<br/>
/// <b>Note:</b> An <see cref="ITag"/> should be used to tag a group of multiple entities. See Remarks.
/// </summary>
/// <remarks>
/// In case you want to find a unique entity add the component <see cref="UniqueEntity"/> to an entity<br/>
/// and use <see cref="EntityStoreBase.GetUniqueEntity"/> to query for this entity.<br/>
/// <br/>
/// Optionally attribute the implementing struct with <see cref="TagNameAttribute"/><br/>
/// to assign a custom tag name used for JSON serialization.
/// </remarks>
public interface ITag { }