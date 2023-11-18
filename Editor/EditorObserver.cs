using System.Collections.Generic;

// ReSharper disable ConvertToAutoProperty
namespace Friflo.Fliox.Editor;

public abstract class EditorObserver
{
    protected           Editor  Editor  => editor;
        
    private             bool    editorReadyFired; 
    internal  readonly  Editor  editor;
    
    protected EditorObserver(Editor editor) {
        this.editor = editor;
    }
        
    protected virtual void OnEditorReady() { }
    
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
        foreach (var editorEvent in observers) {
            editorEvent.SendEditorReady();
        }
    }
}