// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class InspectorControl : UserControl
{
    public InspectorControl()
    {
        InitializeComponent();
    }
    
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        var editor = this.GetEditor();
        editor?.AddObserver(new InspectorObserver(this, editor));
    }
}