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

    private             string          text;
    private readonly    ComponentField  componentField;
    internal            FieldData       data;
    
    public  string   Value { get => text; set => Set(ValueProperty, ref text, value); }
    
    private void Set(DirectPropertyBase<string> property, ref string field, string value) {
        componentField?.UpdateValue(data, value);
        SetAndRaise(property, ref field, value);
    }

    public StringField()
    {
        InitializeComponent();
    }
    
    internal StringField(ComponentField field)
    {
        componentField = field;
        InitializeComponent();
    }
}