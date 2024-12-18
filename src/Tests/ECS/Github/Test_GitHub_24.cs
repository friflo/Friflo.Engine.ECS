using System;
using Friflo.Engine.ECS;
using NUnit.Framework;

/*
// ReSharper disable InconsistentNaming
namespace Tests.ECS.Github
{
    // https://github.com/friflo/Friflo.Engine.ECS/issues/24
    public static class Test_GitHub_24
    {
        public struct Comp_1 : IComponent { }
        public struct Comp_2 : IComponent { }
        public struct Comp_3 : IComponent { }
        public struct Comp_4 : IComponent { }
        
        [Test]
        public static void Exe_GitHub_24()
        {
            var store = new EntityStore();
            var _query = store.Query<Comp_1>()
                .WithoutAllComponents(ComponentTypes.Get<Comp_2>());
            
            var cb = store.GetCommandBuffer();
            var brandNewEntt = store.CreateEntity();
            brandNewEntt.AddComponent<Comp_1>();
            cb.ReuseBuffer = true;
            
            // _query.Archetypes.Length == 1
            foreach (var (c1, e) in _query.Chunks)
            {
                Console.WriteLine(_query.Chunks.Count + " First Log");
                for (int i = 0; i < e.Length; i++)
                {
                    var enttId = e[i];
                    cb.AddComponent<Comp_2>(enttId);
                    
                    var newCB = store.GetCommandBuffer();
                    newCB.AddComponent(enttId,new Comp_3());
                    newCB.AddComponent(enttId, new Comp_4());
                    newCB.Playback();
                }
            }
            cb.Playback();
            
            brandNewEntt.DeleteEntity();
            brandNewEntt = store.CreateEntity();
            brandNewEntt.AddComponent<Comp_1>();
            
            // _query.Archetypes.Length == 2
            // _query.Archetypes[0] = [Comp_1]                  entities: 1
            // _query.Archetypes[1] = [Comp_1, Comp_3, Comp_4]  entities: 0
            foreach (var (c1, e) in _query.Chunks)
            {
                Console.WriteLine(_query.Chunks.Count + " Second Log");
                for (int i = 0; i < e.Length; i++)
                {
                    var enttId = e[i];
                    cb.AddComponent<Comp_2>(enttId);
                    
                    var newCB = store.GetCommandBuffer();
                    newCB.AddComponent(enttId,new Comp_3());
                    newCB.AddComponent(enttId, new Comp_4());
                    newCB.Playback();
                    // now: _query.Archetypes[1] = [Comp_1, Comp_3, Comp_4]  entities: 1
                }
            }
            cb.Playback();
        }
        
        [Test]
        public static void Exe_GitHub_24_fix()
        {
            var store = new EntityStore();
            var _query = store.Query<Comp_1>()
                .WithoutAllComponents(ComponentTypes.Get<Comp_2>());
            
            var cb = store.GetCommandBuffer();
            var brandNewEntt = store.CreateEntity();
            brandNewEntt.AddComponent<Comp_1>();
            cb.ReuseBuffer = true;
            
            // _query.Archetypes.Length == 1
            foreach (var (c1, e) in _query.Chunks)
            {
                Console.WriteLine(_query.Chunks.Count + " First Log");
                for (int i = 0; i < e.Length; i++)
                {
                    var enttId = e[i];
                    cb.AddComponent<Comp_2>(enttId);
                    
                    var newCB = store.GetCommandBuffer();
                    newCB.AddComponent(enttId,new Comp_3());
                    newCB.AddComponent(enttId, new Comp_4());
                    newCB.Playback();
                }
            }
            cb.Playback();
            
            brandNewEntt.DeleteEntity();
            brandNewEntt = store.CreateEntity();
            brandNewEntt.AddComponent<Comp_1>();
            
            // _query.Archetypes.Length == 2
            // _query.Archetypes[0] = [Comp_1]                  entities: 1
            // _query.Archetypes[1] = [Comp_1, Comp_3, Comp_4]  entities: 0
            
            var entities =_query.ToEntityList();
            foreach (var  e in entities)
            {
                Console.WriteLine(entities.Count + " Third Log");

                var enttId = e.Id;
                cb.AddComponent<Comp_2>(enttId);
                
                var newCB = store.GetCommandBuffer();
                newCB.AddComponent(enttId,new Comp_3());
                newCB.AddComponent(enttId, new Comp_4());
                newCB.Playback();
                // now: _query.Archetypes[1] = [Comp_1, Comp_3, Comp_4]  entities: 1
            }
            cb.Playback();
        }
    }
}
*/