// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class InspectorControl : UserControl
{
    internal readonly InspectorControlModel model = new InspectorControlModel();
    
    public InspectorControl()
    {
        DataContext = model;
        InitializeComponent();
        
        TagGroup.GroupAdd.AddSchemaTypes("tags");
        ComponentGroup.GroupAdd.AddSchemaTypes("components");
        ScriptGroup.GroupAdd.AddSchemaTypes("scripts");
        //
        if (!EditorUtils.IsDesignMode) {
            Tags.Children.Clear();
            Components.Children.Clear();
            Scripts.Children.Clear();
        }
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        // designer example data
        if (EditorUtils.IsDesignMode) {
            model.EntityId          = 123456789;
            model.TagCount          = 4;
            model.ComponentCount    = 3;
            model.ScriptCount       = 1;
        }
        var editor = this.GetEditor();
        editor?.AddObserver(new InspectorObserver(this, editor));
    }
}