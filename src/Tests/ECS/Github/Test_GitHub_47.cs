using System;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.Utils;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Github
{
    /// https://github.com/friflo/Friflo.Engine.ECS/issues/47
    public static class Test_GitHub_47
    {
        [Test]
        public static void Parallel_QueryJob_using_AsSpan()
        {
            const int threads = 2;
            Console.WriteLine("Threads count: "+ threads);
            var runner  = new ParallelJobRunner(threads);
            var store = new EntityStore() {JobRunner = runner};
            for (int n = 0; n < 64; n++) {
                store.CreateEntity(new MyComponent1 { a = n });
            }
             
            var query = store.Query<MyComponent1>();
            var queryJob = query.ForEach((chunk, _) =>
            {
                Span<int> values = chunk.AsSpan256<int>();
                for (int i = 0; i < values.Length; i++) {
                    Mem.AreEqual(chunk[i].a, values[i]);
                }
            });
            queryJob.MinParallelChunkLength = 16;
            queryJob.RunParallel();
        }
    }
}
