namespace Friflo.Fliox.Editor;

public static class Program
{
    public static void Main(string[] args)
    {
        var editor = new Editor();
        editor.Init(args).Wait();
        editor.Run();
    }
}

