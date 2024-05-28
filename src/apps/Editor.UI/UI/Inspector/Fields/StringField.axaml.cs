// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public partial class StringField : UserControl, IFieldControl
{
    public static readonly DirectProperty<StringField, string> ValueProperty = AP.RegisterDirect<StringField, string>(nameof(Value), o => o.Value, (o, v) => o.Value = v);

    private     string          text;
    private     string          initText;
    private     bool            modified;
    public      ComponentField  ComponentField { get; init; }
    
    public  string   Value { get => text; set => Set(ValueProperty, ref text, value); }
    
    public void InitValue(string value) {
        Value       = value;
        initText    = value;
        modified    = false;
    }
    
    private void Set(DirectPropertyBase<string> property, ref string field, string value) {
        // ComponentField?.SetString(value);
        modified = true;
        SetAndRaise(property, ref field, value);
    }

    public StringField()
    {
        InitializeComponent();
    }

    protected override void OnKeyDown(KeyEventArgs e) {
        base.OnKeyDown(e);
        if (e.Key == Key.Return && e.KeyModifiers == KeyModifiers.None) {
            ChangeComponentField();
        }
        if (e.Key == Key.Escape && e.KeyModifiers == KeyModifiers.None) {
            Value       = initText;
            modified    = false;
        }
    }

    protected override void OnLostFocus(RoutedEventArgs e) {
        base.OnLostFocus(e);
        ChangeComponentField();
    }
    
    private void ChangeComponentField() {
        if (!modified) {
            return;
        }
        modified    = false;
        initText    = text;
        ComponentField?.SetString(text);
    }
}