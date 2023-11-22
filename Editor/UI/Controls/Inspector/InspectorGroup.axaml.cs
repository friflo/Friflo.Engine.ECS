// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable UnusedParameter.Local
namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

public partial class InspectorGroup : UserControl
{
    private List<InspectorComponent> components;
    
    public static readonly StyledProperty<string>   GroupProperty   = AP.Register<InspectorGroup, string>(nameof(Group), "group");
    public static readonly StyledProperty<Visual>   ExpandProperty  = AP.Register<InspectorGroup, Visual>(nameof(Expand));
    
    public string   Group   { get => GetValue(GroupProperty);   set => SetValue(GroupProperty,  value); }
    public Visual   Expand  { get => GetValue(ExpandProperty);  set => SetValue(ExpandProperty, value); }
    
    public InspectorGroup()
    {
        InitializeComponent();
    }

    private void Button_OnClick(object sender, RoutedEventArgs e) {
        components ??= new List<InspectorComponent>();
        EditorUtils.GetControls(Expand, components);
        var expanded = true;
        foreach (var component in components) {
            expanded &= component.Expanded;
        }
        foreach (var component in components) {
            component.Expanded = !expanded;
        }
    }
    
    bool entered;

    private async void InputElement_OnPointerEntered(object sender, PointerEventArgs e) {
        entered = true;
        await Task.Delay(50);
        if (entered) {
            Add.IsVisible = true;
        }
    }

    private void InputElement_OnPointerExited(object sender, PointerEventArgs e) {
        entered = false;
        Add.IsVisible = false;
    }
}

