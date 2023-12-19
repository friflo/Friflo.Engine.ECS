// Copyright (c) Ullrich Praetz. All rights reserved.
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
    protected           Editor      Editor  => editor;
    protected           EntityStore Store   => editor.Store;
    #endregion
        
#region private fields
    private             bool        editorReadyFired; 
    private   readonly  Editor      editor;
    #endregion

#region construtor
    protected EditorObserver(Editor editor) {
        this.editor = editor;
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
    internal static void CastEditorReady(List<EditorObserver> observers)
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