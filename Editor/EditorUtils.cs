using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Friflo.Fliox.Editor.UI;

namespace Friflo.Fliox.Editor;

public static class EditorUtils
{
    public static bool IsDesignMode => Avalonia.Controls.Design.IsDesignMode;
    
    public static void Post(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
    
    public static async Task InvokeAsync(Func<Task> action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }
}

public static class EditorExtensions
{
    public static Editor GetEditor(this Visual visual, Action onReady = null)
    {
        if (visual.GetVisualRoot() is MainWindow mainWindow) {
            var editor = mainWindow.Editor;
            editor.HandleOnReady(onReady);
            return editor;
        }
        if (EditorUtils.IsDesignMode) {
            return null;
        }
        throw new InvalidOperationException($"{nameof(GetEditor)}() expect {nameof(MainWindow)} as visual root");
    } 
}