// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Numerics;
using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public partial class Vector3Field : UserControl, IFieldControl
{
    public static readonly DirectProperty<Vector3Field, float> XProperty = AP.RegisterDirect<Vector3Field, float>(nameof(X), o => o.X, (o, v) => o.X = v);
    public static readonly DirectProperty<Vector3Field, float> YProperty = AP.RegisterDirect<Vector3Field, float>(nameof(Y), o => o.Y, (o, v) => o.Y = v);
    public static readonly DirectProperty<Vector3Field, float> ZProperty = AP.RegisterDirect<Vector3Field, float>(nameof(Z), o => o.Z, (o, v) => o.Z = v);

    private Vector3         vector;
    public  ComponentField  ComponentField { get; init; }
    
    public  float   X { get => vector.X; set => Set(XProperty, ref vector.X, value); }
    public  float   Y { get => vector.Y; set => Set(YProperty, ref vector.Y, value); }
    public  float   Z { get => vector.Z; set => Set(ZProperty, ref vector.Z, value); }
    
    private void Set(DirectPropertyBase<float> property, ref float field, float value) {
        SetAndRaise(property, ref field, value);
        ComponentField?.SetVector(vector);
    }
    
    public Vector3Field()
    {
        InitializeComponent();
    }
}