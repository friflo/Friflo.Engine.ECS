// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Text;
using static System.Diagnostics.DebuggerBrowsableState;
using Browse = System.Diagnostics.DebuggerBrowsableAttribute;

// ReSharper disable UseCollectionExpression
// ReSharper disable once CheckNamespace
namespace Friflo.Engine.ECS.Systems
{
    internal sealed class View
    {
        public  Tick                    Tick        => system.Tick;
        public  int                     Id          => system.Id;
        public  bool                    Enabled     => system.Enabled;
        public  string                  Name        => system.Name;
        public  SystemRoot              SystemRoot  => system.SystemRoot;
        public  SystemGroup             ParentGroup => system.ParentGroup;
        public  SystemPerf              Perf        => system.perf;

        public override string          ToString()  => $"Enabled: {Enabled}  Id: {Id}";

        [Browse(Never)] private readonly BaseSystem   system;
        
        internal View(BaseSystem system) {
            this.system = system;
        }
    }
        
    internal class SystemGroupDebugView
    {
        public ReadOnlyList<BaseSystem>     ChildSystems    => group.childSystems;
    //  public View                         System          => new View(group);
        
        [Browse(Never)] private readonly SystemGroup group;
        
        internal SystemGroupDebugView(SystemGroup group) {
            this.group = group;
        }
    }
    
    internal class SystemRootDebugView
    {
        public ReadOnlyList<BaseSystem>     ChildSystems    => root.childSystems;
        public ReadOnlyList<EntityStore>    Stores          => root.stores;
    //  public View                         System          => new View(root);
        
        [Browse(Never)] private readonly SystemRoot root;
        
        internal SystemRootDebugView(SystemRoot root) {
            this.root = root;
        }
    }
    
    internal struct Item
    {
        [Browse(RootHidden)] private BaseSystem system;

        public override string ToString()
        {
            var sb = new StringBuilder();
            sb.Append(system.id);
            sb.Append(" - ");
            switch (system) {
                case QuerySystem querySystem:
                    sb.Append(system.Name);
                    sb.Append(" - entities: ");
                    sb.Append(querySystem.EntityCount);
                    break;
                case SystemGroup:
                    sb.Append(system);
                    break;
                default:
                    sb.Append(system.Name);
                    break;
            }
            return sb.ToString();
        }
        
        internal static Item[] GetAllSystems(SystemGroup systemGroup)
        {
            var systemBuffer = new ReadOnlyList<BaseSystem>(Array.Empty<BaseSystem>());
            systemGroup.GetSubSystems(ref systemBuffer);
            var result = new Item[systemBuffer.Count];
            for (int n = 0; n < systemBuffer.Count; n++) {
                result[n] = new Item { system = systemBuffer[n] };   
            }
            return result;
        }
    }
}