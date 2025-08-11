using System;
using System.IO;
using System.Text;
using CodeGen.Query;

namespace CodeGen;

public static class Program {

    public static void Main(string[] args)
    {
        var dir = Path.GetFullPath("../../../../../src/ECS/", Directory.GetCurrentDirectory());
        Console.WriteLine($"Generate at: {dir}");
        for (int n = 2; n <= 3; n++)
        {
            var iEach = QueryGen.IEach_generator(n);
            Write(dir, $"Query/Arg.{n}/IEach.txt", iEach);
            
            var query = QueryGen.Query_generator(n);
            Write(dir, $"Query/Arg.{n}/Query.txt", query);
            
            var queryChunks = QueryGen.Query_Chunks_generator(n);
            Write(dir, $"Query/Arg.{n}/Query.Chunks.txt", queryChunks);
            
            var queryJob = QueryGen.QueryJob_generator(n);
            Write(dir, $"Query/Arg.{n}/QueryJob.txt", queryJob);
        }
    }
    
    private static void Write(string dir, string path, string src)
    {
        Console.WriteLine($"  {path}");
        File.WriteAllText($"{dir}{path}", src, Encoding.UTF8);
    }
}


