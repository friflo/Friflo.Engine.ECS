using System;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Friflo.Fliox.Engine.ECS.Collections;

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
    
    private class InspectorObserver : EditorObserver
    {
        private readonly InspectorControl inspector;
        
        internal InspectorObserver (InspectorControl inspector, Editor editor) : base (editor) { this.inspector = inspector; }

        protected override void OnSelectionChanged(ExplorerItem selection) {
            var text = selection.ToString();
            inspector.TestLabel.Content = text;
            Console.WriteLine($"Inspector.OnSelectionChanged() - {selection}");
        }
    }
}