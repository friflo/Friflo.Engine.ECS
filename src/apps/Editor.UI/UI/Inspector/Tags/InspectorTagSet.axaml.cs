// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI.Inspector;

public partial class InspectorTagSet : UserControl, IExpandable
{
    public static readonly StyledProperty<bool>     ExpandedProperty        = AP.Register<InspectorTagSet, bool>  (nameof(Expanded), true);

    public bool     Expanded        { get => GetValue(ExpandedProperty);        set => SetValue(ExpandedProperty,       value); }
    
    public InspectorTagSet()
    {
        InitializeComponent();
    }
}