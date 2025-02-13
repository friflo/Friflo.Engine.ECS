/*
using System;
using Friflo.Engine.ECS;
using Friflo.Engine.ECS.Systems;
using NUnit.Framework;

// ReSharper disable InconsistentNaming
namespace Tests.ECS.Github {

    struct Hero : ITag { }

    struct Zombie : ITag { }

    struct Monster : ITag { }

    struct Health : IComponent
    {
        public float CurValue;
        public Tags DeathTag;

        public override string ToString() => $"{CurValue}";
    }

    class KillHeroSystem : QuerySystem<Health>
    {
        public Tags[] _etags = Array.Empty<Tags>();

        protected override void OnUpdate()
        {
            var buffer = CommandBuffer;
            Query.WithoutAnyTags(Tags.Get<Zombie, Monster>())
                .ForEachEntity(
                    (ref Health health, Entity entity) =>
                    {
                        health.CurValue--;
                        if (health.CurValue <= 0)
                        {
                            // entity.AddTags(health.DeathTag); 
                            // entity.RemoveTags(Tags.Get<Hero>()); // << here is the problem
                            
                            entity.RemoveTags(Tags.Get<Hero>());
                            entity.AddTags(health.DeathTag);
                            
                            // buffer.RemoveTags(entity.Id, Tags.Get<Hero>());
                            // buffer.AddTags(entity.Id, health.DeathTag);
                            
                            // buffer.AddTags(entity.Id, health.DeathTag);
                            // buffer.RemoveTags(entity.Id, Tags.Get<Hero>());
                            
                            _etags[entity.Id - 1] = health.DeathTag;
                            Console.WriteLine( $"Hero{entity.Id} -> {health.DeathTag.ToString()}");
                        }
                        else
                        {
                            Console.WriteLine($"Hero{entity.Id}.health = {health.CurValue}");
                        }
                    }
                );
        }
    }

    // https://github.com/friflo/Friflo.Engine.ECS/issues/58
    public static class Test_GitHub_58
    {
        [Test]
        public static void Test_GitHub_58_Main()
        {
            for (var i = 0; i < 100; i++)
            {
                Exec();
                Console.WriteLine("Exec:" + i);
            }
        }

        private static void Exec()
        {
            var world = new EntityStore();

            const int n = 5;
            var etags = new Tags[n];

            var rnd = new Random(123);

            for (var i = 0; i < n; i++)
            {
                int health = (int)(5 * rnd.NextDouble()) + 3;
                var e = world.CreateEntity();
                e.AddTag<Hero>();
                e.Add(
                    new Health
                    {
                        CurValue = health,
                        DeathTag = rnd.NextDouble() > 0.5 ? Tags.Get<Zombie>() : Tags.Get<Monster>(),
                    }
                );
            }

            Console.WriteLine(world.Query().AllTags(Tags.Get<Hero>()).Count);

            var root = new SystemRoot(world) { new KillHeroSystem { _etags = etags } };

            while (world.Query().AllTags(Tags.Get<Hero>()).Count > 0)
            {
                root.Update(default);

                foreach (var entity in world.Query().Entities)
                {
                    if (!entity.Tags.HasAll(etags[entity.Id - 1]))
                    {
                        Console.WriteLine(
                            $"Oops Hero{entity.Id}.tags = {entity.Tags} , excepted: {etags[entity.Id - 1]}"
                        );
                        Assert.Fail();
                    }
                }
                // System.Threading.Thread.Sleep(500);
            }
            Console.WriteLine("Game Over");
        }
    }
}
*/


/* --- Results on GameOver ---
 
entity.RemoveTags(Tags.Get<Hero>());
entity.AddTags(health.DeathTag);

[0] = {Entity} id: 4  [Health, #Zombie]
[1] = {Entity} id: 5  [Health, #Zombie]
[2] = {Entity} id: 1  [Health, #Zombie]
[3] = {Entity} id: 2  [Health, #Monster]
[4] = {Entity} id: 3  [Health, #Monster]


entity.AddTags(health.DeathTag); 
entity.RemoveTags(Tags.Get<Hero>()); // << here is the problem

[0] = {Entity} id: 1  [Health, #Hero]
[1] = {Entity} id: 2  [Health, #Hero]
[2] = {Entity} id: 3  [Health, #Hero]
[3] = {Entity} id: 4  [Health, #Monster]
[4] = {Entity} id: 5  [Health, #Zombie]


buffer.RemoveTags(entity.Id, Tags.Get<Hero>());
buffer.AddTags(entity.Id, health.DeathTag);

[0] = {Entity} id: 4  [Health, #Monster]
[1] = {Entity} id: 3  [Health, #Monster]
[2] = {Entity} id: 5  [Health, #Zombie]
[3] = {Entity} id: 2  [Health, #Zombie]
[4] = {Entity} id: 1  [Health, #Zombie]


buffer.AddTags(entity.Id, health.DeathTag);
buffer.RemoveTags(entity.Id, Tags.Get<Hero>());

[0] = {Entity} id: 4  [Health, #Monster]
[1] = {Entity} id: 3  [Health, #Monster]
[2] = {Entity} id: 5  [Health, #Zombie]
[3] = {Entity} id: 2  [Health, #Zombie]
[4] = {Entity} id: 1  [Health, #Zombie]
 
 */
