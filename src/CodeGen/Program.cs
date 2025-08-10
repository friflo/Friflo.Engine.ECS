using System;
using System.IO;
using System.Text;
using CodeGen.Query;

namespace CodeGen;

public class Program {

    public static void Main(string[] args)
    {
        Console.WriteLine("Generate code");
        var dir = Path.GetFullPath("../../../../../src/ECS/", Directory.GetCurrentDirectory());
        for (int n = 2; n <= 2; n++)
        {
            var iEach = QueryGen.IEach_generator(n);
            File.WriteAllText($"{dir}/Query/Arg.{n}/IEach.txt", iEach, Encoding.UTF8);
            
            var query = QueryGen.Query_generator(n);
            File.WriteAllText($"{dir}/Query/Arg.{n}/Query.txt", query, Encoding.UTF8);
            
            var queryChunks = QueryGen.Query_Chunks_generator(n);
            File.WriteAllText($"{dir}/Query/Arg.{n}/Query.Chunks.txt", queryChunks, Encoding.UTF8);
            
            var queryJob = QueryGen.QueryJob_generator(n);
            File.WriteAllText($"{dir}/Query/Arg.{n}/QueryJob.txt", queryJob, Encoding.UTF8);
        }
    }    
}


