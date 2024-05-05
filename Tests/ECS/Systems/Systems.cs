using System.Diagnostics;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using static NUnit.Framework.Assert;

// ReSharper disable NotAccessedField.Local
namespace Tests.ECS.Systems
{
    public class TestSystem1 : QuerySystem<Position>
    {
        protected override void OnUpdate() {
            Query.ForEachEntity((ref Position position, Entity entity) => {
                position.x++;
                CommandBuffer.AddComponent(entity.Id, new Scale3(4,5,6));
            });
            AreEqual(42, Tick.deltaTime);
        }

        protected override void OnUpdateGroupEnd() { }
    }
    
    public class TestSystem2 : QuerySystem<Position>
    {
        protected override void OnUpdate() {
            foreach (var (positions, _)  in Query.Chunks) {
                foreach (ref var position in positions.Span) {
                    position.x++;
                }
            }
        }
    }
    
    public class ScaleSystem : QuerySystem<Scale3>
    {
        protected override void OnUpdate() {
            Query.ForEachEntity((ref Scale3 scale3, Entity _) => {
                scale3.x++;
            });
        }
    }
    
    public class PositionSystem : QuerySystem<Position>
    {
        protected override void OnUpdate() {
            Query.ForEachEntity((ref Position position, Entity _) => {
                position.x++;
            });
        }
    }
    
    public class TestGroup : SystemGroup {
        internal int beginCalled;
        internal int endCalled;
        
        public TestGroup() : base("TestGroup") { }

        protected override void OnUpdateGroupBegin() {
            AreEqual(1, SystemRoot.Stores.Count);
            AreEqual(42, Tick.deltaTime);
            beginCalled++;
        }

        protected override void OnUpdateGroupEnd() {
            AreEqual(1, SystemRoot.Stores.Count);
            endCalled++;
        }
    }
    
    // Ensure a custom System class can be declared without any overrides
    public class MySystem1 : BaseSystem { }
    
    // A custom System class with all possible overrides
    public class MySystem2 : BaseSystem {
        public      override string Name => "MySystem2 - custom name";
        
        protected   override void   OnUpdateGroupBegin() { }
        protected   override void   OnUpdateGroupEnd()   { }
        public      override void   Update(Tick tick)    { }
    }
    
    public class PerfSystem: BaseSystem {
        private int dummy;

        /// <summary>
        /// Do some loop iterations to ensure <see cref="BaseSystem.durationTicks"/> > 0.<br/>
        /// Execution time must be greater than 1 / <see cref="Stopwatch.Frequency"/>.
        /// </summary>
        public override void Update(Tick tick) {
            for (int n = 0; n < 1000; n++) {
                dummy++;
            }
        }
    }
}