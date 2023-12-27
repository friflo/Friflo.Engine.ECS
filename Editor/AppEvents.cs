using System.Collections.Generic;
using Friflo.Engine.ECS;

namespace Friflo.Editor;

public abstract class AppEvents
{
    protected readonly  List<EditorObserver>    observers   = new List<EditorObserver>();
    protected           bool                    isReady;
    
    public void AddObserver(EditorObserver observer) ////
    {
        observers.Add(observer);
        if (isReady) {
            observer.SendEditorReady();  // could be deferred to event loop
        }
    }
    
    public void SelectionChanged(EditorSelection selection) {   ////
        StoreDispatcher.Post(() => {
            EditorObserver.CastSelectionChanged(observers, selection);    
        });
    }
}