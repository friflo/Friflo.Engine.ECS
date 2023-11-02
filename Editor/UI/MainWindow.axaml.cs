using System;
using System.Threading.Tasks;
using Avalonia.Controls;

namespace Friflo.Fliox.Editor.UI;

public partial class MainWindow : Window
{
    private Editor editor;
    
    public MainWindow()
    {
        InitializeComponent();
        OpenGLControl.OpenGlReady = OpenGlReady;
    }

    public override void Show() {
        base.Show();
        Console.WriteLine($"--- MainWindow.Show() - startup {Program.startTime.ElapsedMilliseconds} ms");
    }

    protected override void OnClosed(EventArgs e) {
        editor?.Shutdown();
        base.OnClosed(e);
    }

    private void OpenGlReady()
    {
        Task.Run(async () => {
            editor = new Editor();
            await editor.Init();
            Console.WriteLine($"--- MainWindow.OpenGlReady() - Editor.Init() {Program.startTime.ElapsedMilliseconds} ms");
        });
        Console.WriteLine($"--- MainWindow.OpenGlReady() {Program.startTime.ElapsedMilliseconds} ms");
    }

}