using System;
using System.Collections;
using System.Collections.Generic;
using Friflo.Engine.ECS;
using NUnit.Framework;
using static NUnit.Framework.Assert;

// ReSharper disable UnusedVariable
// ReSharper disable RedundantTypeDeclarationBody
// ReSharper disable HeuristicUnreachableCode
// ReSharper disable InconsistentNaming
// ReSharper disable once CheckNamespace
namespace Tests.ECS {


public static class Test_Script
{
    private const long Count = 10; // 1_000_000_000L
    
    [Test]
    public static void Test_1_Scripts_default() {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        
        AreEqual(0, entity.Scripts.Length);
    }
    
    private static void OnScriptAdded(ScriptChanged args)
    {
        switch (args.Script) {
            case TestScript1 script1:
                break;
            case TestScript2 script2:
                break;
        }
    }
    
    [Test]
    public static void Test_1_add_remove_Script()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        AreEqual("id: 1  []",   player.ToString());
        AreSame(store,          player.Archetype.Store);
        
        var script1 = new TestScript1 { val1 = 1 };
        var script2 = new TestScript2 { val2 = 2 };
        var script3 = new TestScript2 { val2 = 3 };

        // --- add script handler
        var addCount    = 0;
        var addHandler  = new Action<ScriptChanged>(args => {
            var str = args.ToString();
            switch (addCount++) {
                case 0:     AreEqual(1,                         args.Entity.Id);
                            AreSame (store,                     args.Store);
                            AreEqual(ScriptChangedAction.Add,   args.Action);
                            AreSame (script1,                   args.Script);
                            AreEqual(typeof(TestScript1),       args.ScriptType.Type);
                            // Ensure Scripts are updated
                            AreEqual(1,                         args.Entity.Scripts.Length);
                            AreEqual("entity: 1 - event > Add Script: [*TestScript1]",  str);   return;
                case 1:     AreEqual("entity: 1 - event > Replace Script: [*TestScript2]",  str);return;
                case 2:     AreEqual("entity: 1 - event > Replace Script: [*TestScript2]",  str);return;
                default:    Fail("unexpected event");                                           return;
            }
        });
        // --- add script handler
        var removeCount    = 0;
        var removeHandler  = new Action<ScriptChanged>(args => {
            var str = args.ToString();
            switch (removeCount++) {
                case 0:     AreEqual(1,                         args.Entity.Id);
                            AreEqual(ScriptChangedAction.Remove,args.Action);
                            AreEqual(typeof(TestScript2),       args.ScriptType.Type);
                            IsNull  (                           args.Script);
                            AreSame (script3,                   args.OldScript);
                            IsNull  (script3.Store);
                            // Ensure Scripts are updated                            
                            AreEqual(1,                         args.Entity.Scripts.Length);
                            AreEqual("entity: 1 - event > Remove Script: [*TestScript2]",   str);   return;
                default:    Fail("unexpected event");                                               return;
            }
        });
        store.OnScriptAdded    += addHandler;
        store.OnScriptAdded    += OnScriptAdded;
        store.OnScriptRemoved  += removeHandler;
        
        // --- add script type: TestScript1
        IsNull(player.AddScript(script1));
        NotNull(script1.Entity);
        AreSame(script1,        player.GetScript<TestScript1>());
        AreEqual(1,             player.Scripts.Length);
        AreEqual("id: 1  [*TestScript1]", player.ToString());
        AreEqual(1,             player.Scripts.Length);
        AreSame (script1,       player.Scripts[0]);
        AreEqual(1,             store.EntityScripts.Length);
        
        var e = Throws<InvalidOperationException> (() => {
            player.AddScript(script1);
        });
        AreEqual("script already added to an entity. current entity id: 1", e!.Message);
        AreEqual(1,             player.Scripts.Length);
        
        // --- add script type: TestScript2
        IsNull (player.AddScript(script2));
        NotNull (script2.Entity);
        
        AreSame (script2,       player.GetScript<TestScript2>());
        AreEqual(2,             player.Scripts.Length);
        AreEqual("id: 1  [*TestScript1, *TestScript2]", player.ToString());
        AreEqual(1,             store.EntityScripts.Length);
        
        // --- add script type that already exists

        NotNull (player.AddScript(script3));   // will send event
        IsTrue  (script2.Entity.IsNull);
        IsFalse (script3.Entity.IsNull);
        AreSame (script3,       player.GetScript<TestScript2>());
        AreEqual(2,             player.Scripts.Length);
        AreEqual("id: 1  [*TestScript1, *TestScript2]", player.ToString());
        
        // --- remove Script
        player.RemoveScript<TestScript2>();
        AreEqual(1,             player.Scripts.Length);
        
        // --- remove non-existing Script
        player.RemoveScript<TestScript2>();
        AreEqual(1,             player.Scripts.Length); // no event sent
        
        store.OnScriptAdded    -= addHandler;
        store.OnScriptRemoved  -= removeHandler;
        
        for (long n = 0; n < Count; n++) {
            _ = player.GetScript<TestScript1>();
        }
        AreEqual(3, addCount);      // last assertion ensuring no events sent in perf test
        AreEqual(1, removeCount);   // last assertion ensuring no events sent in perf test
    }
    
    [Test]
    public static void Test_2_RemoveScript() {
        var store   = new EntityStore(PidType.RandomPids);
        var player = store.CreateEntity();
        
        var script1 = new TestScript1();
        IsFalse(player.TryGetScript<TestScript1>(out _));
        IsNull(player.RemoveScript<TestScript1>());
        AreEqual("id: 1  []",               player.ToString());
        AreEqual(0,                         player.Scripts.Length);
        AreEqual("[*TestScript1]",          script1.ToString());
        
        player.AddScript(script1);
        AreEqual(1,                         player.Scripts.Length);
        AreSame (script1, player.GetScript<TestScript1>());
        IsTrue  (player.TryGetScript<TestScript1>(out var result));
        AreSame (script1, result);
        AreEqual("id: 1  [*TestScript1]", player.ToString());
        NotNull (script1.Entity);
        IsFalse (player.TryGetScript<TestScript2>(out _));
        
        NotNull (player.RemoveScript<TestScript1>());
        AreEqual(0,                         player.Scripts.Length);
        IsNull  (player.GetScript<TestScript1>());
        IsFalse (player.TryGetScript<TestScript1>(out _));
        AreEqual("id: 1  []",               player.ToString());
        IsTrue  (script1.Entity.IsNull);
        
        IsNull(player.RemoveScript<TestScript1>());
        AreEqual(0,                         player.Scripts.Length);
    }
    
    [Test]
    public static void Test_3_RemoveScript() {
        var store   = new EntityStore(PidType.RandomPids);
        var player = store.CreateEntity();
        
        IsNull  (player.AddScript(new TestScript1 { val1 = 1 }));
        IsNull  (player.AddScript(new TestScript2 { val2 = 2 }));
        IsNull  (player.AddScript(new TestScript3 { val3 = 3 }));
        NotNull (player.RemoveScript<TestScript2>());
        AreEqual(2, player.Scripts.Length);
        
        NotNull(player.GetScript<TestScript1>());
        IsNull (player.GetScript<TestScript2>());
        NotNull(player.GetScript<TestScript3>());
    }
    
    /// <summary>Cover move last script in <see cref="StoreExtension.RemoveScript"/> </summary>
    [Test]
    public static void Test_3_cover_move_last_script() {
        var store   = new EntityStore(PidType.RandomPids);
        var entity1 = store.CreateEntity();
        var entity2 = store.CreateEntity();
        
        IsNull  (entity1.AddScript(new TestScript1 { val1 = 1 }));
        IsNull  (entity2.AddScript(new TestScript2 { val2 = 2 }));
        AreEqual(1,                         entity1.Scripts.Length);
        AreEqual(1,                         entity2.Scripts.Length);
        AreEqual(2,                         store.EntityScripts.Length);
        
        // --- test EntityScriptsEnumerator
        {
            int count = 0;
            int count2 = 0;
            foreach (var entityScripts in store.EntityScripts) {
                count2 += entityScripts.Count;
                foreach (var _ in entityScripts) {
                    count++;       
                }
            }
            AreEqual(2, count);
            AreEqual(2, count2);
        } {
            int count = 0;
            foreach (var entityScripts in store.EntityScripts) {
                IEnumerable<Script> scripts = entityScripts;
                var enumerator = scripts.GetEnumerator();
                while (enumerator.MoveNext()) {
                    count++;                    
                }
                enumerator.Reset();
                while (enumerator.MoveNext()) {
                    count++;                    
                }
                enumerator.Dispose();
            }
            AreEqual(4, count);
        } {
            int count = 0;
            foreach (var entityScripts in store.EntityScripts) {
                IEnumerable scripts = entityScripts;
                foreach (var _ in scripts) {
                    count++;                    
                }
            }
            AreEqual(2, count);
        }
        
        NotNull (entity1.RemoveScript<TestScript1>());
        AreEqual(0,                         entity1.Scripts.Length);
        AreEqual(1,                         store.EntityScripts.Length);
        NotNull (entity2.RemoveScript<TestScript2>());
        AreEqual(0,                         entity2.Scripts.Length);
        AreEqual(0,                         store.EntityScripts.Length);
        
        IsNull  (entity1.GetScript<TestScript1>());
        IsNull  (entity2.GetScript<TestScript2>());
    }
    
    [Test]
    public static void Test_Script_Enumerator()
    {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        var script  = new TestScript1 { val1 = 1 };
        AreEqual(0,             entity.Scripts.Length);
        AreEqual("Script[0]",   entity.Scripts.ToString());
        
        IsNull  (entity.AddScript(script));
        
        AreEqual(1,             entity.Scripts.Length);
        AreEqual(1,             entity.Scripts.Span.Length);
        AreEqual("Script[1]",   entity.Scripts.ToString());
        {
            IEnumerable<Script> enumerable = entity.Scripts;
            int count = 0;
            foreach (var item in enumerable) {
                count++;
                AreSame(script, item);
            }
            AreEqual(1, count);
        } {
            IEnumerable enumerable = entity.Scripts;
            var enumerator = enumerable.GetEnumerator();
            using var disposable = enumerator as IDisposable;
            int count = 0;
            enumerator.Reset();
            while (enumerator.MoveNext()) {
                count++;
                AreSame(script, enumerator.Current);
            }
            AreEqual(1, count);
        }
    }
    
    /// <summary>Cover <see cref="EntityUtils.RemoveScript"/></summary>
    [Test]
    public static void Test_3_cover_remove_non_added_script() {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        
        IsNull  (entity.AddScript(new TestScript1 { val1 = 1 }));
        AreEqual(1, entity.Scripts.Length);
        
        IsNull  (entity.RemoveScript<TestScript2>());
        AreEqual(1, entity.Scripts.Length); // remains unchanged
    }
    
    [Test]
    public static void Test_2_Perf() {
        var store   = new EntityStore(PidType.RandomPids);
        var list = new List<Entity>();
        for (long n = 0; n < 10; n++) {
            list.Add(store.CreateEntity());
        }
        IsTrue(list.Count > 0);
    }
    
    [Test]
    public static void Test_GetScript_Perf() {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        player.AddScript(new TestScript1());
        NotNull(player.GetScript<TestScript1>());
        
        const int count = 10; // 1_000_000_000 ~ #PC: 3.269 ms
        for (long n = 0; n < count; n++) {
            player.GetScript<TestScript1>();
        }
    }
    
    [Test]
    public static void Test_3_Perf_Add_Remove_Component() {
        var store   = new EntityStore(PidType.RandomPids);
        var player  = store.CreateEntity();
        AreEqual("id: 1  []", player.ToString());
        
        const int count = 10; // 100_000_000 ~ #PC: 3.791 ms
        for (long n = 0; n < count; n++) {
            var script1 = new TestScript1();
            player.AddScript(script1);
            player.RemoveScript<TestScript1>();
        }
    }
    
    [ComponentKey("empty")]
    private class EmptyScript : Script { }
    
    [Test]
    public static void Test_Empty_Lifecycle_methods() {
        var empty = new EmptyScript();
        empty.Start();
        empty.Update();
    }
    
    /* Editor Inspector would look like
    
    Entity              id 0    
    > TestComponent     health 4
    > Position          x 1     y 0     z 0
    > MyComponent1      a 1
         
    */
    [Test]
    public static void Test_3_Simulate_Editor() {
        var store   = new EntityStore(PidType.RandomPids);
        var entity  = store.CreateEntity();
        
        var test    = new TestComponent();
        entity.AddScript(test);                         // component added via editor
        entity.AddComponent(new Position { x = 1 });    // script added via editor
        entity.AddComponent(new MyComponent1 { a = 1}); // script added via editor
        
        AreEqual(1, entity.Scripts.Length);
        AreEqual(2, entity.Archetype.ComponentCount);
        AreEqual("id: 1  [Position, MyComponent1, *TestComponent]", entity.ToString());
        IsTrue  (entity == test.Entity);
        test.Start();
        test.Update();
    }
}

}



