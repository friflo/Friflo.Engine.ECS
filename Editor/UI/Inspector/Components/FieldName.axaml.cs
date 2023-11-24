// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class FieldName : UserControl
{
    public static readonly StyledProperty<string>   TextProperty  = AP.Register<FieldName, string>(nameof(Text), "name");
    
    public string   Text  { get => GetValue(TextProperty);  set => SetValue(TextProperty, value); }
    
    public FieldName()
    {
        InitializeComponent();
    }
}