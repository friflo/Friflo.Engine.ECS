// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Collections;

// Note! Must not using Avalonia namespaces

// ReSharper disable ConvertToAutoProperty
namespace Friflo.Editor;

public struct EditorSelection {
    public ExplorerItem item;
}

public abstract class EditorObserver
{
#region protected properties
    protected           EntityStore Store       => appEvents.Store;
    #endregion
        
#region private fields
    private             bool        editorReadyFired; 
    private   readonly  AppEvents   appEvents;
    #endregion

#region construtor
    protected EditorObserver(AppEvents appEvents) {
        this.appEvents = appEvents;
    }
    #endregion
        
#region events
    protected virtual   void    OnEditorReady()                                     { }
    protected virtual   void    OnSelectionChanged(in EditorSelection selection)    { }
    #endregion
    
    // -------------------------------------- send methods --------------------------------------
#region send methods
    internal void SendEditorReady()
    {
        if (editorReadyFired) {
            return;
        }
        editorReadyFired = true;
        OnEditorReady();
    }
    #endregion
    
    
    // -------------------------------------- cast methods --------------------------------------
#region cast methods
    public static void CastEditorReady(List<EditorObserver> observers)
    {
        foreach (var observer in observers) {
            observer.SendEditorReady();
        }
    }
    
    internal static void CastSelectionChanged(List<EditorObserver> observers, in EditorSelection selection)
    {
        foreach (var observer in observers) {
            observer.OnSelectionChanged(selection);
        }
    }
    #endregion
}