using System.Collections.Generic;
using Friflo.Fliox.Engine.ECS.Collections;

// ReSharper disable ConvertToAutoProperty
namespace Friflo.Fliox.Editor;


public struct EditorSelection {
    public ExplorerItem item;
}

public abstract class EditorObserver
{
    protected           Editor  Editor  => editor;
        
    private             bool    editorReadyFired; 
    private   readonly  Editor  editor;
    
    protected EditorObserver(Editor editor) {
        this.editor = editor;
    }
        
    protected virtual   void    OnEditorReady()                                     { }
    protected virtual   void    OnSelectionChanged(in EditorSelection selection)    { }
    
    internal void SendEditorReady()
    {
        if (editorReadyFired) {
            return;
        }
        editorReadyFired = true;
        OnEditorReady();
    }
    
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
}