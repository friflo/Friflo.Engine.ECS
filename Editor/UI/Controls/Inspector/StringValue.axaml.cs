// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

public partial class StringValue : UserControl
{
    public static readonly StyledProperty<string>   ValueProperty  = AP.Register<InspectorComponent, string>(nameof(Value), "value");
    
    public string   Value  { get => GetValue(ValueProperty);  set => SetValue(ValueProperty, value); }
    
    public StringValue()
    {
        InitializeComponent();
    }
}