using System;
using Friflo.Engine.ECS;
using UnityEngine;

public class AnimateMeshInstanced : MonoBehaviour
{
    private ArchetypeQuery<Position> query;
    private QueryJob<Position> job;
    private ParallelJobRunner runner;

    public Material material;
    public Mesh mesh;
    const int numInstances = 100_000;
    float delta;
    RenderParams rp;
    Matrix4x4[] instData;

    void Start()
    {
        rp = new RenderParams(material);
        instData = new Matrix4x4[numInstances];
        runner = new ParallelJobRunner(8);
        var store = new EntityStore() { JobRunner = runner };
        // create entities with a Position component
        for (int n = 0; n < numInstances; n++) {
            store.Batch()
                .Add(new Position(n, 0, 0))
                .CreateEntity();    
        }
        query = store.Query<Position>();
        job = query.ForEach((positions, entities) => {
            UpdatePositions(positions.Span);
        });
    }


    private void UpdatePositions(Span<Position> positions) {
        int i = 0;
        foreach (ref var position in positions) {
            position.y += delta;
            instData[i] = Matrix4x4.Translate(new Vector3(-4.5f + i++, position.y, 0f));
        }
    }
    
    public bool runParallel = false;

    void Update()
    {
        delta = Time.deltaTime / 4;
        if (runParallel) {
            job.RunParallel();
        } else {
            foreach (var (positions, entities) in query.Chunks) {
                UpdatePositions(positions.Span);
            }
        }
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
    }

    private void OnDestroy() {
        runner.Dispose();
    }
}
