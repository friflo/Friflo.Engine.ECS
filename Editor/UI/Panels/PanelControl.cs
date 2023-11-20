using Avalonia.Controls;
using Avalonia.Input;

namespace Friflo.Fliox.Editor.UI.Panels;

public class PanelControl : UserControl
{
    public PanelControl() {
        Focusable = true;
    }
    protected override void OnGotFocus(GotFocusEventArgs e) {
        base.OnGotFocus(e);
        EditorUtils.SetActivePanel(this);
    }

    protected override void OnPointerPressed(PointerPressedEventArgs e) {
        base.OnPointerPressed(e);
        EditorUtils.SetActivePanel(this);
        Focus();
    }
    
    public virtual bool OnEvent(string ev) => false;
}