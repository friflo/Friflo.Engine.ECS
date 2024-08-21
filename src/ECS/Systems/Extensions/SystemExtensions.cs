// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Diagnostics;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems;

/// <summary>
/// Contains System extension methods.
/// </summary>
public static class SystemExtensions
{
    internal static readonly double StopwatchPeriodMs = 1 / (Stopwatch.Frequency / 1000d);
    
    private static SystemMatch[]    _matches = new SystemMatch[1];
    private static int              _matchesCount;
    
    /// <summary>
    /// Return the systems of a <see cref="SystemGroup"/> matching the passed <paramref name="archetype"/>.
    /// </summary>
    public static void GetMatchingSystems(this SystemGroup systemGroup, Archetype archetype, List<SystemMatch> target, bool addGroups)
    {
        if (systemGroup == null)    throw new ArgumentNullException(nameof(systemGroup));
        if (target == null)         throw new ArgumentNullException(nameof(target));
        if (archetype == null)      throw new ArgumentNullException(nameof(archetype));
        
        target.Clear();
        var entityStore = archetype.entityStore;
        _matchesCount   = 0;
        var stores      = systemGroup.SystemRoot.Stores;
        for (int storeIndex = 0; storeIndex < stores.Count; storeIndex++)   // commonly: one or few stores per ECSSystemSet
        {
            var store = stores[storeIndex];
            // early out if different EntityStore
            if (store != entityStore) {
                continue;
            }
            if (addGroups) {
                var parent = -1;
                AddMatch(new SystemMatch { system = systemGroup, depth = 0, parent = parent });
                GetGroupSystems(archetype, systemGroup, storeIndex, parent, 1);
            } else {
                GetSystems(archetype, systemGroup, storeIndex);
            }
        }
        AddMatches(target, addGroups);
        // Clear system references to enable collecting them by GC
        for (int n = 0; n < _matchesCount; n++) {
            _matches[n].system = null;
        }
    }
    
    private static void AddMatches(List<SystemMatch> target, bool addGroups)
    {
        if (addGroups) {
            for (int n = 0; n < _matchesCount; n++) {
                ref var match = ref _matches[n];
                if (match.count == 0) {
                    continue;
                }
                target.Add(match);
            }
            return;
        }
        for (int n = 0; n < _matchesCount; n++) {
            target.Add(_matches[n]);
        }
    }
    
    private static void GetGroupSystems(Archetype type, SystemGroup group, int storeIndex, int parent, int depth)
    {
        foreach (var system in group.ChildSystems)            // commonly: a dozen or hundreds of systems
        {
            if (system is SystemGroup systemGroup) {
                var subParent = _matchesCount;
                AddMatch(new SystemMatch { system = systemGroup, depth = depth, parent = parent });
                GetGroupSystems(type, systemGroup, storeIndex, subParent, depth + 1);
                continue;
            }
            if (system is QuerySystemBase querySystem) {
                var query = querySystem.Queries[storeIndex];
                if (query.IsMatch(type.ComponentTypes, type.Tags)) {
                    IncrementCount(parent);
                    AddMatch(new SystemMatch { system = querySystem, depth = depth, parent = parent, count = 1 });
                }    
            }
        }
    }
    
    private static void GetSystems(Archetype type, SystemGroup group, int storeIndex)
    {
        foreach (var system in group.ChildSystems)            // commonly: a dozen or hundreds of systems
        {
            if (system is SystemGroup systemGroup) {
                GetSystems(type, systemGroup, storeIndex);
                continue;
            }
            if (system is QuerySystemBase querySystem) {
                var query = querySystem.Queries[storeIndex];
                if (query.IsMatch(type.ComponentTypes, type.Tags)) {
                    AddMatch(new SystemMatch { system = querySystem, count = 1 });
                }    
            }
        }
    }
    
    private static void AddMatch(in SystemMatch match)
    {
        if (_matchesCount == _matches.Length) {
            ArrayUtils.Resize(ref _matches, 2 * _matchesCount);
        }
        _matches[_matchesCount++] = match;
    }

    private static void IncrementCount(int parent)
    {
        while (true) {
            if (parent == -1) {
                return;
            }
            ref var match = ref _matches[parent];
            match.count++;
            parent = match.parent;
        }
    }
}
