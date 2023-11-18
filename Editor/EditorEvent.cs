using System.Collections.Generic;

namespace Friflo.Fliox.Editor;

public abstract class EditorEvent
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
    
    internal static void CastEditorReady(List<EditorEvent> editorEvents)
    {
        foreach (var editorEvent in editorEvents) {
            editorEvent.SendEditorReady();
        }
    }
}