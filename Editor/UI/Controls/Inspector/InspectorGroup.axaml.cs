// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using AP = Avalonia.AvaloniaProperty;

namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

public partial class InspectorGroup : UserControl
{
    public static readonly StyledProperty<string>   GroupProperty       = AP.Register<InspectorComponent, string>(nameof(Group), "group");
    public static readonly StyledProperty<bool>     ExpandedProperty    = AP.Register<InspectorComponent, bool>  (nameof(Expanded), true);
    
    public string   Group  { get => GetValue(GroupProperty);  set => SetValue(GroupProperty, value); }
    public bool     Expanded        { get => GetValue(ExpandedProperty);        set => SetValue(ExpandedProperty,       value); }
    
    public InspectorGroup()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e) {
        Expanded = !Expanded;
    }
}