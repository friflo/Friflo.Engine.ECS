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
    public static readonly StyledProperty<Position>   ValueProperty  = AP.Register<InspectorComponent, Position>(nameof(Value), new Position(1, 2, 3));
    
    public Position   Value  { get => GetValue(ValueProperty);  set => SetValue(ValueProperty, value); }
    
    public PositionField()
    {
        InitializeComponent();
    }
}