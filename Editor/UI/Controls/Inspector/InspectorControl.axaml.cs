using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

public partial class InspectorControl : UserControl
{
    public InspectorControl()
    {
        InitializeComponent();
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        var editor = EditorUtils.GetEditor(this);
        editor?.AddObserver(new InspectorObserver(this, editor));
    }
    
    // ------------------------------------------- EditorObserver -------------------------------------------
    private class InspectorObserver : EditorObserver
    {
        private readonly InspectorControl inspector;
        
        internal InspectorObserver (InspectorControl inspector, Editor editor) : base (editor) { this.inspector = inspector; }

        protected override void OnSelectionChanged(in EditorSelection selection)
        {
            var item    = selection.item;
            var text    = item?.Name ?? "no selection";
            var id      = item != null ? $"id: {item.Id}" : null;
            inspector.EntityName.Content    = text;
            inspector.EntityId.Content      = id;
        }
    }
}