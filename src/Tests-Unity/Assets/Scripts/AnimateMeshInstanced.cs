using System;
using Friflo.Engine.ECS;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class AnimateMeshInstanced : MonoBehaviour
{
    [SerializeField] private TMP_Text count;
    [SerializeField] private TMP_Text fpsText;
    [SerializeField] private Toggle   parallel;

    
    private ArchetypeQuery<Position> query;
    private QueryJob<Position> job;
    private ParallelJobRunner runner;

    public Material material;
    public Mesh mesh;
    private const int NumInstances = 100_000;
    float delta;
    RenderParams rp;
    Matrix4x4[] instData;
    
    void Start()
    {
        count.text = $"Count: {NumInstances}";
        rp = new RenderParams(material);
        instData = new Matrix4x4[NumInstances];
        runner = new ParallelJobRunner(8);
        var store = new EntityStore() { JobRunner = runner };
        // create entities with a Position component
        for (int n = 0; n < NumInstances; n++) {
            store.Batch()
                .Add(new Position(n, 0, 0))
                .CreateEntity();    
        }
        query = store.Query<Position>();
        job = query.ForEach((positions, _) => {
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

    private const int FPSSampleCount = 30;
    private readonly int[] fpsSamples = new int[FPSSampleCount];
    private int sampleIndex;
    public bool runParallel;

    private void UpdateFps()
    {
        var sum = 0;
        for (var i = 0; i < FPSSampleCount; i++)
        {
            sum += fpsSamples[i];
        }

        fpsText.text = $"FPS: {sum / FPSSampleCount}";
    }
    
    void Update()
    {
        fpsSamples[sampleIndex++] = (int)(1.0f / Time.deltaTime);
        if (sampleIndex >= FPSSampleCount) sampleIndex = 0;
        
        UpdateFps();
        
        
        delta = Time.deltaTime / 4;
        if (runParallel) {
            job.RunParallel();
        } else {
            foreach (var (positions, _) in query.Chunks) {
                UpdatePositions(positions.Span);
            }
        }
        Graphics.RenderMeshInstanced(rp, mesh, 0, instData);
        
        parallel.onValueChanged .AddListener((dd) => {
            
            runParallel = dd;
            // runParallel = _toggle.isOn;
        });
    }

    private void OnDestroy() {
        runner.Dispose();
    }
}
