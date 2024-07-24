// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using System;
using Avalonia.Controls;
using Friflo.Engine.ECS;

// ReSharper disable UnusedParameter.Local
// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI;

public partial class MainWindow : Window
{
    public              Editor  Editor { get; init; }

    public MainWindow()
    {
        InitializeComponent();
        OpenGlControl.OpenGlReady = OpenGlReady;
    }

    // ReSharper disable once RedundantOverriddenMember
    /// <summary>Is the last call into user code before the event loop is entered</summary>
    public override void Show() {
        base.Show();
        StoreDispatcher.Post(() =>
        {
            ExplorerPanel.TreeDataGrid.Focus();    
        });
        
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

    private void CopyToClipboard(object sender, EventArgs e) {
        Editor.ExecuteCommand(new CopyToClipboardCommand());
    }
}