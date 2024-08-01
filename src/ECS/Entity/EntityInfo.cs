// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS;

/// <summary>
/// Used to provide additional debug information for an <see cref="Entity"/>:<br/>
/// <see cref="Entity.Pid"/>                <br/>
/// <see cref="Entity.Enabled"/>            <br/>
/// <see cref="Entity.Archetype"/>          <br/>
/// <see cref="Entity.Scripts"/>            <br/>
/// <see cref="Entity.Parent"/>            <br/>
/// <see cref="Entity.DebugJSON"/>          <br/>
/// <see cref="Entity.DebugEventHandlers"/> <br/>
/// <see cref="Entity.Revision"/>           <br/>
/// </summary>
internal readonly struct EntityInfo
{
    #region properties
    internal            long                Pid             => entity.Pid;
    internal            bool                Enabled         => entity.Enabled;
    internal            Archetype           Archetype       => entity.GetArchetype();
    internal            Scripts             Scripts         => entity.Scripts;
    internal            Entity              Parent          => entity.Parent;
    internal            JSON                JSON            => new JSON(EntityUtils.EntityToJSON(entity));
    internal            DebugEventHandlers  EventHandlers   => EntityStore.GetEventHandlers(entity.store, entity.Id);
    internal            EntityLinks         IncomingLinks   => entity.GetAllIncomingLinks();
    internal            short               Revision        => entity.Revision;
    public   override   string              ToString()      => GetString();
    #endregion

    [Browse(Never)] private readonly Entity entity;
    
    internal EntityInfo(Entity entity) {
        this.entity = entity;
    }
    
    private string GetString()
    {
        var incomingLinks = entity.CountAllIncomingLinks();
        var outgoingLinks = entity.CountAllOutgoingLinks();
        if (incomingLinks == 0 && outgoingLinks == 0) {
            return "";
        }
        var sb = new StringBuilder();
        sb.Append("links incoming: ");
        sb.Append(incomingLinks);
        sb.Append(" outgoing: ");
        sb.Append(outgoingLinks);
        return sb.ToString();
    }
}

/// <summary>
/// Struct used to display the entity data as JSON in debugger when expanded.<br/>
/// </summary>
internal readonly struct JSON
{
    // ReSharper disable once InconsistentNaming
    internal readonly   string  Value;
    public   override   string  ToString() => "";

    internal JSON(string json) {
        Value = json;
    }
}
