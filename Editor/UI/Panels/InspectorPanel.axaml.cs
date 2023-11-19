// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia.Controls;
using Avalonia.Interactivity;

namespace Friflo.Fliox.Editor.UI.Panels;

public partial class InspectorPanel : UserControl
{
    public InspectorPanel()
    {
        InitializeComponent();
    }

    // ReSharper disable once RedundantOverriddenMember
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
    }
}