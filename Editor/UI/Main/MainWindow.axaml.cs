// Copyright (c) Ullrich Praetz. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;

// ReSharper disable once CheckNamespace
namespace Friflo.Fliox.Editor.UI;

public partial class MainWindow : Window
{
    public              Editor  Editor { get; }

    public MainWindow()
    {
        Editor = new Editor();
        InitializeComponent();
        OpenGlControl.OpenGlReady = OpenGlReady;
    }

    // ReSharper disable once RedundantOverriddenMember
    /// <summary>Is the last call into user code before the event loop is entered</summary>
    public override void Show() {
        base.Show();
        // Console.WriteLine($"--- MainWindow.Show() - startup {Program.startTime.ElapsedMilliseconds} ms");
    }

    protected override void OnClosed(EventArgs e) {
        Editor?.Shutdown();
        base.OnClosed(e);
    }

    private async void OpenGlReady()
    {
        /* Task.Run(async () => {
            await Editor.Init();
            Console.WriteLine($"--- MainWindow.OpenGlReady() - Editor.Init() {Program.startTime.ElapsedMilliseconds} ms");
        }); */
        await Editor.Init();

        Console.WriteLine($"--- MainWindow.OpenGlReady() {Program.startTime.ElapsedMilliseconds} ms");
    }

    private void QuitProgramCommand(object sender, EventArgs e) {
        Close();
    }
}