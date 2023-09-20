using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics.Systems;
using UnityEngine;

public struct DebugInput : IInputComponentData
{
    public InputEvent DebugEvent;
}

public struct DebugTag : IComponentData
{
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct DebugSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);

        new DebugJob
        {
            ECB = commandBuffer,
            ShouldSetInputTag = state.GetComponentLookup<DebugTag>(true)
        }.Schedule();

        state.Dependency.Complete();
        
        commandBuffer.Playback(state.EntityManager);
    }
}


[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[BurstCompile]
public partial struct ServerDebugSystem : ISystem
{
    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        
        new HandleDebugJob
        {
            
        }.Schedule();
        
        state.Dependency.Complete();
        
        commandBuffer.Playback(state.EntityManager);
        
    }
}

[BurstCompile]
public partial struct DebugJob : IJobEntity
{
    [ReadOnly]
    public ComponentLookup<DebugTag> ShouldSetInputTag;
    public EntityCommandBuffer ECB;

    [BurstCompile]
    private void Execute(Entity entity, ref DebugInput debugInput)
    {
        Debug.Log($"Running Debug Job");
        debugInput.DebugEvent = default;

        bool shouldSetInput = ShouldSetInputTag.HasComponent(entity);
        if (shouldSetInput)
        {
            Debug.Log($"Setting input");
            debugInput.DebugEvent.Set();
            ECB.RemoveComponent<DebugTag>(entity);
        }
    }
}

[BurstCompile]
public partial struct HandleDebugJob : IJobEntity
{
    
    [BurstCompile]
    private void Execute(Entity entity, ref DebugInput debugInput)
    {
        Debug.Log($"Running HandleDebugJob Job");
        
        if (debugInput.DebugEvent.IsSet)
        {
            Debug.Log($"Debug Event is set. Count: {debugInput.DebugEvent.Count}");
        }
    }
}