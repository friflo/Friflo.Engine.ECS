// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

public partial class PositionField : UserControl
{
    public static readonly StyledProperty<float>   XProperty  = AP.Register<PositionField, float>(nameof(X), 1);
    public static readonly StyledProperty<float>   YProperty  = AP.Register<PositionField, float>(nameof(Y), 2);
    public static readonly StyledProperty<float>   ZProperty  = AP.Register<PositionField, float>(nameof(Z), 3);
    
    public float   X  { get => GetValue(XProperty);  set => SetValue(XProperty, value); }
    public float   Y  { get => GetValue(YProperty);  set => SetValue(YProperty, value); }
    public float   Z  { get => GetValue(ZProperty);  set => SetValue(ZProperty, value); }
    
    public PositionField()
    {
        InitializeComponent();
    }
}