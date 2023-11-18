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
    
    public static void AssertUIThread()
    {
        Dispatcher.UIThread.VerifyAccess();
    }

    public static void Post(Action action)
    {
        Dispatcher.UIThread.Post(action);
    }
    
    public static async Task InvokeAsync(Func<Task> action)
    {
        await Dispatcher.UIThread.InvokeAsync(action);
    }
    
    public static Editor GetEditor(Visual visual)
    {
        if (visual.GetVisualRoot() is MainWindow mainWindow) {
            return mainWindow.Editor;
        }
        if (IsDesignMode) {
            return null;
        }
        throw new InvalidOperationException($"{nameof(GetEditor)}() expect {nameof(MainWindow)} as visual root");
    } 
}
