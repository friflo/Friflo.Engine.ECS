// Copyright (c) Ullrich Praetz - https://github.com/friflo. All rights reserved.
// See LICENSE file in the project root for full license information.

using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Friflo.Editor.Utils;
using Friflo.Engine.ECS;

// ReSharper disable once CheckNamespace
namespace Friflo.Editor.UI;

public class App : Application
{
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.Startup += (_, _) => {
                // Console.WriteLine($"--- ApplicationLifetime - startup {Program.startTime.ElapsedMilliseconds} ms");
            };            
            desktop.MainWindow = AppEvents.CreateMainWindow();
        }

        base.OnFrameworkInitializationCompleted();
        StoreDispatcher.SetDispatcher(new AvaloniaDispatcher());
    }
}