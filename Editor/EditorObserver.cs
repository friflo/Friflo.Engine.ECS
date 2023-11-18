using System.Collections.Generic;

namespace Friflo.Fliox.Editor;

public abstract class EditorObserver
{
    private bool editorReadyFired; 
        
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