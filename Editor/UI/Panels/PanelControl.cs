// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Panels;

public class PanelControl : UserControl
{
    internal    PanelHeader Header => header;
    
    private     Editor      editor;
    private     PanelHeader header;
    
    protected PanelControl() {
        Focusable = true;
    }
    
    protected override void OnGotFocus(GotFocusEventArgs e) {
        base.OnGotFocus(e);
        editor.SetActivePanel(this);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        editor.SetActivePanel(this);
        // Focus(); - calling Focus() explicit corrupt navigation with Key.Tab
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        editor = this.GetEditor();
        header = EditorUtils.FindControl<PanelHeader>(this);
    }
    
    public virtual bool OnExecuteCommand(EditorCommand command) => false;
}