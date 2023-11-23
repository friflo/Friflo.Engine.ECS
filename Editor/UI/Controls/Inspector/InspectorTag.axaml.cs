// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

namespace Friflo.Fliox.Editor.UI.Controls.Inspector;

public partial class InspectorTag : UserControl
{
    public static readonly StyledProperty<string>   TagNameProperty  = AP.Register<InspectorTag, string>(nameof(TagName), "Tag");
    
    public string   TagName  { get => GetValue(TagNameProperty);  set => SetValue(TagNameProperty, value); }
    
    public InspectorTag()
    {
        InitializeComponent();
    }
}