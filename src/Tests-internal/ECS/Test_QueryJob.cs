using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Friflo.Engine.ECS;
using NUnit.Framework;
using Tests.ECS;
using static NUnit.Framework.Assert;

// ReSharper disable CoVariantArrayConversion
// ReSharper disable once InconsistentNaming
namespace Internal.ECS {

public static class Test_QueryJob
{
    private static void AssertMissingRunner(QueryJob job)
    {
        job.jobRunner = null;
        var e = Throws<InvalidOperationException>(job.RunParallel)!;
        AreEqual("QueryJob requires a JobRunner", e.Message);
    }
    
    [Test]
    public static void Test_QueryJob_ToString()
    {
        var store   = new EntityStore(PidType.UsePidAsId);
        var types   = ComponentTypes.Get<MyComponent1, MyComponent2, Position, Rotation, Scale3>();
        var archetype   = store.GetArchetype(types);
        for (int n = 0; n < 32; n++) {
            archetype.CreateEntity();
        }
        {
            var job = store.Query<MyComponent1>().ForEach((_, _) => { });
            AreEqual(32, job.Chunks.Count);
            AreEqual(32, job.Entities.Count);
            AreEqual("QueryJob [MyComponent1]", job.ToString());
            AssertMissingRunner(job);
        } {
            var job = store.Query<MyComponent1, MyComponent2>().ForEach((_, _, _) => { });
            AreEqual(32, job.Chunks.Count);
            AreEqual(32, job.Entities.Count);
            AreEqual("QueryJob [MyComponent1, MyComponent2]", job.ToString());
            AssertMissingRunner(job);
        } {
            var job = store.Query<MyComponent1, MyComponent2, Position>().ForEach((_, _, _, _) => { });
            AreEqual(32, job.Chunks.Count);
            AreEqual(32, job.Entities.Count);
            AreEqual("QueryJob [MyComponent1, MyComponent2, Position]", job.ToString());
            AssertMissingRunner(job);
        } {
            var job = store.Query<MyComponent1, MyComponent2, Position, Rotation>().ForEach((_, _, _, _, _) => { });
            AreEqual(32, job.Chunks.Count);
            AreEqual(32, job.Entities.Count);
            AreEqual("QueryJob [MyComponent1, MyComponent2, Position, Rotation]", job.ToString());
            AssertMissingRunner(job);
        } {
            var job = store.Query<MyComponent1, MyComponent2, Position, Rotation, Scale3>().ForEach((_, _, _, _, _, _) => { });
            AreEqual(32, job.Chunks.Count);
            AreEqual(32, job.Entities.Count);
            AreEqual("QueryJob [MyComponent1, MyComponent2, Position, Rotation, Scale3]", job.ToString());
            AssertMissingRunner(job);
        }
    }

    [Test]
    public static void Test_QueryJob_LeastCommonMultiple()
    {
        AreEqual(  0, QueryJob.LeastCommonMultiple( 0,  1));
        AreEqual(  0, QueryJob.LeastCommonMultiple( 1,  0));
        
        AreEqual(  1, QueryJob.LeastCommonMultiple( 1,  1));
        AreEqual(  2, QueryJob.LeastCommonMultiple( 1,  2));
        AreEqual(  6, QueryJob.LeastCommonMultiple( 2,  3));
        AreEqual(  6, QueryJob.LeastCommonMultiple( 3,  2));
        
        AreEqual(192, QueryJob.LeastCommonMultiple(12, 64));
        AreEqual(320, QueryJob.LeastCommonMultiple(20, 64));
        AreEqual(192, QueryJob.LeastCommonMultiple(24, 64));
        AreEqual(448, QueryJob.LeastCommonMultiple(28, 64));
        AreEqual(320, QueryJob.LeastCommonMultiple(40, 64));
        AreEqual(192, QueryJob.LeastCommonMultiple(48, 64));
        AreEqual(448, QueryJob.LeastCommonMultiple(56, 64));
    }
    
    [Test]
    public static void Test_QueryJob_ComponentMultiple()
    {
        AreEqual( 64, ComponentType<ByteComponent>  .ComponentMultiple);
        AreEqual( 32, ComponentType<ShortComponent> .ComponentMultiple);
        AreEqual( 16, ComponentType<IntComponent>   .ComponentMultiple);
        AreEqual(  8, ComponentType<LongComponent>  .ComponentMultiple);
        //
        AreEqual(  4, ComponentType<Component16>    .ComponentMultiple);
        AreEqual(  2, ComponentType<Component32>    .ComponentMultiple);
        AreEqual(  1, ComponentType<Component64>    .ComponentMultiple);
        //
        AreEqual( 16, ComponentType<Position>       .ComponentMultiple); // 12 bytes
        AreEqual( 16, ComponentType<Scale3>         .ComponentMultiple); // 12 bytes
        AreEqual(  4, ComponentType<Rotation>       .ComponentMultiple); // 16 bytes
        AreEqual(  1, ComponentType<Transform>      .ComponentMultiple); // 64 bytes
        
        AreEqual( 16, ComponentType<Component20>    .ComponentMultiple);
        
        AreEqual( 20, Unsafe.SizeOf<Component20>());
    }
    
    class MyTask : JobTask
    {
        internal override void ExecuteTask() { }
    }
    
    // [Test]
    public static void Test_QueryJob_ExecuteJob_Perf()
    {
        int count = 100_000_000;
        using var runner = new ParallelJobRunner(8);
        var task = new MyTask();
        var tasks = new [] {
            task,
            task,
            task,
            task,
            task,
            task,
            task,
            task,
        };
        runner.ExecuteJob(new object(), tasks);
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            runner.ExecuteJob(new object(), tasks);
        }
        Console.WriteLine($"ExecuteJob() perf - count: {count}, parallelism: {runner.ThreadCount}, duration: {sw.ElapsedMilliseconds}");
    }
    
    // [Test]
    public static void Test_QueryJob_TPL_Perf()
    {
        int count = 1_000_000;
        Action action = () => {};
        var actions = new[] {
            action,
            action,
            action,
            action,
            action,
            action,
            action,
            action,
        };
        Parallel.Invoke(actions);
        var sw = new Stopwatch();
        sw.Start();
        for (int n = 0; n < count; n++) {
            Parallel.Invoke(actions);
        }
        Console.WriteLine($"Parallel.Invoke() perf - count: {count}, parallelism: {actions.Length}, duration: {sw.ElapsedMilliseconds}");
    }
}

}