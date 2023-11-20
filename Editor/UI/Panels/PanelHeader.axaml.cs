// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

namespace Friflo.Fliox.Editor.UI.Panels;

public partial class PanelHeader : UserControl
{
    public static readonly StyledProperty<string>   PanelTitleProperty  = AP.Register<PanelHeader, string>(nameof(PanelTitle), "Panel Title");
    public static readonly StyledProperty<bool>     PanelActiveProperty = AP.Register<PanelHeader, bool>(nameof(PanelTitle), false);
    
    public string PanelTitle  { get => GetValue(PanelTitleProperty);    set => SetValue(PanelTitleProperty,  value); }
    public bool   PanelActive { get => GetValue(PanelActiveProperty);   set => SetValue(PanelActiveProperty, value); }
    
    public PanelHeader()
    {
        InitializeComponent();
    }
}