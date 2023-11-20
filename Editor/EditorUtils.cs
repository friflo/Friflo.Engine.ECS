﻿// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Friflo.Fliox.Editor.UI;
// ReSharper disable MergeCastWithTypeCheck

namespace Friflo.Fliox.Editor;

public static class EditorUtils
{
    private static bool IsDesignMode => Design.IsDesignMode;
    
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
    
    public static Editor GetEditor(this Visual visual)
    {
        if (visual.GetVisualRoot() is MainWindow mainWindow) {
            return mainWindow.Editor;
        }
        if (IsDesignMode) {
            return null;
        }
        throw new InvalidOperationException($"{nameof(GetEditor)}() expect {nameof(MainWindow)} as visual root");
    }
    
    public static void CopyToClipboard(Visual visual, string text)
    {
        var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
        if (clipboard == null) {
            Console.Error.WriteLine("CopyToClipboard() error: clipboard is null");
            return;
        }
        clipboard.SetTextAsync(text);
        // --- following example snippet didn't work on macOS on first try. In Windows 10 OK
        // var dataObject  = new DataObject();
        // dataObject.Set(DataFormats.Text, text);
        // clipboard.SetDataObjectAsync(dataObject);
    }
    
    internal static T FindControl<T>(Visual control) where T : Control
    {
        foreach (var child in control.GetVisualChildren()) {
            if (child is not Control childControl) {
                continue;
            }
            if (childControl is T) {
                return (T)childControl;
            }
            var sub = FindControl<T>(childControl);
            if (sub != null) {
                return sub;
            }
        }
        return null;
    }
}
