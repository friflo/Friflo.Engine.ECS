/*
using System.Numerics;
using Friflo.Engine.ECS;
using NUnit.Framework;


// ReSharper disable InconsistentNaming
namespace Tests.ECS.Github
{
    public struct LocalTransform : IComponent
    {
        public Matrix4x4 matrix;
    }

    // Small test to check performance of GitHub discussion: "Best practice for update hierarchy's transform?"
    // https://github.com/friflo/Friflo.Engine.ECS/discussions/44
    public static class Test_GitHub_44
    {
        [Test]
        public static void TraverseHierarchy()
        {
            var store = new EntityStore();
            // create a hierarchy of entities 
            var root = store.CreateEntity(new Transform(), new LocalTransform());
            for (int n0 = 0; n0 < 10; n0++) {
                var entity0 = store.CreateEntity(new Transform(), new LocalTransform());
                root.AddChild(entity0);
                for (int n1 = 0; n1 < 10; n1++) {
                    var entity1 = store.CreateEntity(new Transform(), new LocalTransform());
                    entity0.AddChild(entity1);
                     for (int n2 = 0; n2 < 10; n2++) {
                        var entity2 = store.CreateEntity(new Transform(), new LocalTransform());
                        entity1.AddChild(entity2);
                        for (int n3 = 0; n3 < 5; n3++) {
                            var entity3 = store.CreateEntity(new Transform(), new LocalTransform());
                            entity2.AddChild(entity3);
                        }
                    }
                }
            }
            Assert.AreEqual(6111, store.Entities.Count);
            
            // 10,000 iterations traversing 6111 entities: 1.2 sec
            for (int n = 0; n < 10_000; n++) {
                int calls = 0;
                Traverse(root, Matrix4x4.Identity, ref calls);
                Assert.AreEqual(6111, calls);
            }
        }
        
        private static void Traverse (Entity entity, Matrix4x4 parentLocalToWorld, ref int calls)
        {
            calls++;
            if (entity.HasComponent<Transform>()) {
                ref var transform = ref entity.GetComponent<Transform>();
                if (entity.TryGetComponent<LocalTransform>(out var localTransform))
                {
                    transform.value = parentLocalToWorld * localTransform.matrix;
                    parentLocalToWorld = transform.value;
                } else {
                    transform.value = parentLocalToWorld;
                }    
            }
            foreach (var child in entity.ChildEntities) {
                Traverse(child, parentLocalToWorld, ref calls);        
            }
        }
    }
}
*/