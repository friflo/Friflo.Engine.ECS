// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class StringField : UserControl
{
    public static readonly DirectProperty<StringField, string> ValueProperty = AP.RegisterDirect<StringField, string>(nameof(Value), o => o.Value, (o, v) => o.Value = v);

    private string   text;
    
    public  string   Value {
        get => text;
        set => SetAndRaise(ValueProperty, ref text, value);
    }

    public StringField()
    {
        InitializeComponent();
    }
}