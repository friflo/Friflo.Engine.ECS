// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Panels;

public partial class PanelHeader : UserControl
{
    public static readonly StyledProperty<string> PanelTitleProperty =
        AvaloniaProperty.Register<PanelHeader, string>(nameof(PanelTitle), "Panel Title");
    
    public string PanelTitle { get => GetValue(PanelTitleProperty); set => SetValue(PanelTitleProperty, value); }
    
    public PanelHeader()
    {
        InitializeComponent();
    }

    // ReSharper disable once RedundantOverriddenMember
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
    }
}