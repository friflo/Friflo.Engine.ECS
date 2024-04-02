// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using System.Collections;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Friflo.Editor.Utils;
using AP = Avalonia.AvaloniaProperty;

// ReSharper disable once CheckNamespace - fix namespace
namespace Friflo.Editor.UI;

/// <summary>
/// Used to display data validation error in Avalonia control within a <see cref="ToolTip"/>
/// </summary>
public partial class ValidationError : UserControl
{
    public static readonly StyledProperty<IEnumerable>  ItemsSourceProperty = AP.Register<ValidationError, IEnumerable> (nameof(ItemsSource));
    public static readonly StyledProperty<string>       ErrorProperty       = AP.Register<ValidationError, string>      (nameof(Error), "name");

    /// <summary>implement <see cref="ItemsSource"/> similar to <see cref="ItemsControl.ItemsSource"/></summary>
    public IEnumerable  ItemsSource { get => GetValue(ItemsSourceProperty); set => SetValue(ItemsSourceProperty, value); }
    public string       Error       { get => GetValue(ErrorProperty);       set => SetValue(ErrorProperty, value);       }

    public ValidationError()
    {
        InitializeComponent();
    }

    /// <summary>
    /// <see cref="ItemsSource"/> is assigned within App.axaml Style:<br/>
    /// <code>
    ///     Style Selector="DataValidationErrors"
    ///     ...
    ///     ui:ValidationError x:DataType="DataValidationErrors" ItemsSource="{Binding}"
    /// </code>
    /// </summary>
    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);
        if (EditorUtils.IsDesignMode) {
            return;
        }
        foreach (var error in ItemsSource)
        {
            // set error to first error entry
            if (error is Exception exception) {
                Error = exception.Message;
            } else {
                Error = error.ToString();
            }
            break;
        }
    }
}