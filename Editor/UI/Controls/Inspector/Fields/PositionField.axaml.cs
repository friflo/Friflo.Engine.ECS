// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

public partial class PositionField : UserControl
{
    public static readonly DirectProperty<PositionField, string> XProperty = AP.RegisterDirect<PositionField, string>(nameof(X), o => o.x, (o, v) => o.x = v);
    public static readonly DirectProperty<PositionField, string> YProperty = AP.RegisterDirect<PositionField, string>(nameof(Y), o => o.y, (o, v) => o.y = v);
    public static readonly DirectProperty<PositionField, string> ZProperty = AP.RegisterDirect<PositionField, string>(nameof(Z), o => o.z, (o, v) => o.z = v);

    private string   x;
    private string   y;
    private string   z;
    
    public  string   X { get => x; set => SetAndRaise(XProperty, ref x, value); }
    public  string   Y { get => y; set => SetAndRaise(YProperty, ref y, value); }
    public  string   Z { get => z; set => SetAndRaise(ZProperty, ref z, value); }
    
    public PositionField()
    {
        InitializeComponent();
    }
}