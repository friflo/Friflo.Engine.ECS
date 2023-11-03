using System;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Friflo.Fliox.Editor.UI;

namespace Friflo.Fliox.Editor;

public static class EditorUtils
{
    public static void Post(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
}

public static class EditorExtensions
{
    public static Editor GetEditor(this Visual visual)
    {
        if (visual.GetVisualRoot() is MainWindow mainWindow) {
            return mainWindow.Editor;
        }
        if (Avalonia.Controls.Design.IsDesignMode) {
            return null;
        }
        throw new InvalidOperationException($"{nameof(GetEditor)}() expect {nameof(MainWindow)} as visual root");
    } 
}