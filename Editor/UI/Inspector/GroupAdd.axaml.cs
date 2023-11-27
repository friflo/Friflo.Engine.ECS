// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls;
using Friflo.Fliox.Engine.ECS;
using AP = Avalonia.AvaloniaProperty;

namespace Friflo.Fliox.Editor.UI.Inspector;

public partial class GroupAdd : UserControl
{
    public static readonly StyledProperty<string>       GroupNameProperty   = AP.Register<GroupAdd, string>       (nameof(GroupName));
    
    public string       GroupName   { get => GetValue(GroupNameProperty);   set => SetValue(GroupNameProperty,  value); }
    public Entity       Entity      { get; set; }
    
    
    public GroupAdd()
    {
        InitializeComponent();
    }
}