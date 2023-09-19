using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Physics.Systems;
using UnityEngine;

[GhostComponent(PrefabType=GhostPrefabType.AllPredicted, OwnerSendType = SendToOwnerType.SendToNonOwner)]
public struct DebugInput : IInputComponentData
{
    [GhostField] public InputEvent DebugEvent;
}

public struct DebugTag : IComponentData
{
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation, WorldSystemFilterFlags.ClientSimulation)]
[UpdateInGroup(typeof(GhostInputSystemGroup))]
public partial struct DebugSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Debug.Log("On Create DebugSystem");
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        
        state.Dependency.Complete();
        new DebugJob
        {
            ECB = commandBuffer
        }.Run();

        state.Dependency.Complete();
        
        commandBuffer.Playback(state.EntityManager);
        
    }
}


[UpdateInGroup(typeof(PhysicsSystemGroup))]
[UpdateBefore(typeof(PhysicsInitializeGroup))]
[BurstCompile]
public partial struct ServerDebugSystem : ISystem
{
    public void OnCreate(ref SystemState state)
    {
        Debug.Log("On Create ServerDebugSystem");
    }

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
    public EntityCommandBuffer ECB;
    
    [BurstCompile]
    private void Execute(Entity entity, DebugTag debugTag, ref DebugInput debugInput)
    {
        Debug.Log($"Running Debug Job");
        debugInput.DebugEvent.Set();
        ECB.RemoveComponent<DebugTag>(entity);
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