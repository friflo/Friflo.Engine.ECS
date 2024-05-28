// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public partial class FieldLabel : UserControl
{
    public static readonly StyledProperty<string>   TextProperty  = AP.Register<FieldLabel, string>(nameof(Text), "name");
    
    public string   Text  { get => GetValue(TextProperty);  set => SetValue(TextProperty, value); }
    
    public FieldLabel()
    {
        InitializeComponent();
    }
}