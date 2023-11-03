using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Friflo.Fliox.Editor.UI;

public partial class MainWindow : Window
{
    private readonly Editor editor;
    
    public MainWindow()
    {
        editor = new Editor();
        InitializeComponent();
        OpenGLControl.OpenGlReady = OpenGlReady;
    }

    // ReSharper disable once RedundantOverriddenMember
    /// <summary>Is the last call into user code before the event loop is enetered</summary>
    public override void Show() {
        base.Show();
        // Console.WriteLine($"--- MainWindow.Show() - startup {Program.startTime.ElapsedMilliseconds} ms");
    }

    protected override void OnClosed(EventArgs e) {
        editor?.Shutdown();
        base.OnClosed(e);
    }

    private void OpenGlReady()
    {
        Task.Run(async () => {
            await editor.Init();
            Console.WriteLine($"--- MainWindow.OpenGlReady() - Editor.Init() {Program.startTime.ElapsedMilliseconds} ms");
        });
        Console.WriteLine($"--- MainWindow.OpenGlReady() {Program.startTime.ElapsedMilliseconds} ms");
    }

}