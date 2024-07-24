// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System.Collections.Generic;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Friflo.Editor.Utils;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable UnusedParameter.Local
namespace Friflo.Editor.UI.Inspector;


public interface IExpandable
{
    bool Expanded { get; set; }
}

public partial class InspectorGroup : UserControl, IExpandable
{
    public static readonly StyledProperty<bool>     ExpandedProperty        = AP.Register<InspectorTagSet, bool>  (nameof(Expanded), true);

    public bool     Expanded        { get => GetValue(ExpandedProperty);        set => SetValue(ExpandedProperty,       value); }
    
    private List<IExpandable> expandables;
    
    public static readonly StyledProperty<string>       GroupNameProperty   = AP.Register<InspectorGroup, string>       (nameof(GroupName), "items");
    public static readonly StyledProperty<InputElement> ExpandProperty      = AP.Register<InspectorGroup, InputElement> (nameof(Expand));
    public static readonly StyledProperty<int>          CountProperty       = AP.Register<InspectorGroup, int>          (nameof(Count));
    
    public string       GroupName   { get => GetValue(GroupNameProperty);   set => SetValue(GroupNameProperty,  value); }
    public InputElement Expand      { get => GetValue(ExpandProperty);      set => SetValue(ExpandProperty,     value); }
    public int          Count       { get => GetValue(CountProperty);       set => SetValue(CountProperty,      value); }

    public InspectorGroup()
    {
        InitializeComponent();
    }
    
    /*
    protected override void OnLoaded(RoutedEventArgs e) {
        base.OnLoaded(e);
        
        // was used to show add button with GreenButton style only on hover
        // PointerEntered += (sender, args) => { Add.Classes.Add("GreenButton"); };
        // PointerExited += (sender, args)  => { Add.Classes.Remove("GreenButton"); };
        // if (Expand != null) {
        //     Expand.PointerEntered += (sender, args) => { Add.Classes.Add("GreenButton"); };
        //     Expand.PointerExited += (sender, args)  => { Add.Classes.Remove("GreenButton"); };
        // }
    } */

    private void Button_OnClick(object sender, RoutedEventArgs e) {
        expandables ??= new List<IExpandable>();
        expandables.Clear();
        EditorUtils.GetControls(Expand, expandables);
        if (expandables.Count == 0) {
            return;
        }
        Expanded = !Expanded;

        /* if (!Expanded) {
            foreach (var expandable in expandables) {
                expandable.Expanded = false;
            }
            Expanded = true;
            return;
        }
        var hasExpanded = false;
        foreach (var expandable in expandables) {
            hasExpanded    |= expandable.Expanded;
        }
        if (hasExpanded) {
            Expanded = false;   
            return;
        }
        foreach (var expandable in expandables) {
            expandable.Expanded = true;
        } */
    }
}

