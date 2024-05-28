// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Collections;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Rendering;
using Avalonia.VisualTree;

// ReSharper disable MergeCastWithTypeCheck
namespace Friflo.Editor.Utils;

public static class EditorUtils
{
    public static bool IsDesignMode => Design.IsDesignMode;
    
    public static AppEvents GetEditor(this Visual visual)
    {
        var renderRoot = (IRenderRoot)AppEvents.Window;
        if (visual.GetVisualRoot() == renderRoot) {
            return AppEvents.Instance;
        }
        if (IsDesignMode) {
            return null;
        }
        throw new InvalidOperationException($"{nameof(GetEditor)}() expect {nameof(AppEvents)}.{nameof(AppEvents.Window)} as visual root");
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
    
    public static async Task<string> GetClipboardText(Visual visual)
    {
        var clipboard = TopLevel.GetTopLevel(visual)?.Clipboard;
        if (clipboard == null) {
            // ReSharper disable once MethodHasAsyncOverload
            Console.Error.WriteLine("GetClipboardText() error: clipboard is null");
            return null;
        }
        return await clipboard.GetTextAsync();
    }
    
    internal static T FindAncestor<T>(StyledElement control) where T : Control
    {
        while (control != null) {
            if (control is T) {
                return (T)control;
            }
            control = control.Parent;
        }
        return null;
    }
    
    internal static InputElement FindFocusable(Visual control)
    {
        foreach (var child in control.GetVisualChildren()) {
            if (child is InputElement inputElement) {
                if (inputElement.Focusable) {
                    return inputElement;
                }
            }
            var focusableChild = FindFocusable(child);
            if (focusableChild != null) {
                return focusableChild;
            }
        }
        return null;
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
    
    internal static void GetControls<T>(Visual control, List<T> result)
    {
        if (control is T) {
            result.Add((T)(object)control);
        }
        foreach (var child in control.GetVisualChildren()) {
            if (child is not Control childControl) {
                continue;
            }
            GetControls(childControl, result);
        }
    }
    
    private static InputElement GetFocusable(Visual visual)
    {
        if (visual is InputElement inputElement) {
            if (inputElement.Focusable) {
                return inputElement;
            }
        }
        var children = (AvaloniaList<Visual>)visual.GetVisualChildren();
        foreach (var child in children) {
            var focusable = GetFocusable(child);
            if (focusable != null) {
                return focusable;
            }
        }
        return null;
    }

    internal static InputElement GetTabIndex(Visual control, int tabOffset)
    {
        var parent          = control.GetVisualParent()!;
        var children        = parent.GetVisualChildren();
        var inputElements   = new List<Focusable>();
        foreach (var child in children) {
            var focusable = GetFocusable(child);
            if (focusable != null) {
                inputElements.Add(new Focusable { element = child, focusable = focusable });
            }
        }
        var count = inputElements.Count;
        for (int n = 0; n < count; n++) {
            if (control == inputElements[n].element) {
                int next = (n + tabOffset + count) % count;
                return inputElements[next].focusable;
            }
        }
        return null;
    }
}

struct Focusable
{
    internal Visual         element;
    internal InputElement   focusable;
}
