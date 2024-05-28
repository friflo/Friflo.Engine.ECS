// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public partial class ValueField : UserControl, IFieldControl
{
    public static readonly DirectProperty<ValueField, string> ValueProperty = AP.RegisterDirect<ValueField, string>(nameof(Value), o => o.Value, (o, v) => o.Value = v);

    private string          text;
    public  ComponentField  ComponentField { get; init; }
    
    public  string   Value { get => text; set => Set(ValueProperty, ref text, value); }
    
    private void Set(DirectPropertyBase<string> property, ref string field, string value) {
        ComponentField?.SetString(value);
        SetAndRaise(property, ref field, value);
    }

    public ValueField()
    {
        InitializeComponent();
    }
}