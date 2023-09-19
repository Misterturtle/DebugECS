using Unity.Burst;
using Unity.Collections;
using Unity.Entities;
using Unity.NetCode;
using Unity.Transforms;
using UnityEngine;


public struct GoInGameRPC : IRpcCommand
{
    
}

[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ClientSimulation | WorldSystemFilterFlags.ThinClientSimulation)]
[UpdateInGroup(typeof(InitializationSystemGroup))]
public partial struct GoInGameClientSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        Debug.Log("OnCreate Client System");
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<NetworkId>()
            .WithNone<NetworkStreamInGame>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("OnUpdate Client System");
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        
        new ClientConnectJob { ECB =  commandBuffer}.Schedule();
        
        state.Dependency.Complete();
        commandBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile]
[WithNone(typeof(NetworkStreamInGame))]
public partial struct ClientConnectJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    
    [BurstCompile]
    private void Execute(Entity entity, in NetworkId networkId)
    {
        Debug.Log("ClientConnectJob");
        ECB.AddComponent<NetworkStreamInGame>(entity);
        var req = ECB.CreateEntity();
        ECB.AddComponent<GoInGameRPC>(req);
        ECB.AddComponent(req, new SendRpcCommandRequest { TargetConnection = entity });
    }
}


[BurstCompile]
[WorldSystemFilter(WorldSystemFilterFlags.ServerSimulation)]
[UpdateInGroup(typeof(PredictedSimulationSystemGroup))]
public partial struct GoInGameServerSystem : ISystem
{
    [BurstCompile]
    public void OnCreate(ref SystemState state)
    {
        state.RequireForUpdate<Spawner>();
        Debug.Log("OnCreate Server");
        var builder = new EntityQueryBuilder(Allocator.Temp)
            .WithAll<GoInGameRPC>()
            .WithAll<ReceiveRpcCommandRequest>();
        state.RequireForUpdate(state.GetEntityQuery(builder));
    }

    [BurstCompile]
    public void OnUpdate(ref SystemState state)
    {
        Debug.Log("Scheduling ReceiveClientConnectJob");
        var commandBuffer = new EntityCommandBuffer(Allocator.TempJob);
        var playerEntity = SystemAPI.GetSingleton<Spawner>().SpawnableEntity;
        
        new ReceiveClientConnectJob
        {
            ECB = commandBuffer,
            EntityToSpawn = playerEntity,
            NetworkIdLookup = state.GetComponentLookup<NetworkId>(true)
        }.Schedule();
        state.Dependency.Complete();
        commandBuffer.Playback(state.EntityManager);
    }
}

[BurstCompile]
[WithNone(typeof(NetworkStreamInGame))]
public partial struct ReceiveClientConnectJob : IJobEntity
{
    public EntityCommandBuffer ECB;
    public Entity EntityToSpawn;
    
    [ReadOnly]
    public ComponentLookup<NetworkId> NetworkIdLookup;
    
    
    [BurstCompile]
    private void Execute(Entity entity, in ReceiveRpcCommandRequest receiveRpcCommandRequest)
    {
        Debug.Log("ReceiveClientConnectJob");
        ECB.AddComponent<NetworkStreamInGame>(receiveRpcCommandRequest.SourceConnection);
        
        var networkId = NetworkIdLookup[receiveRpcCommandRequest.SourceConnection];

        var player = ECB.Instantiate(EntityToSpawn);
        
        ECB.SetComponent(player, new GhostOwner { NetworkId = networkId.Value });
        ECB.AppendToBuffer(receiveRpcCommandRequest.SourceConnection, new LinkedEntityGroup { Value = player });

        ECB.DestroyEntity(entity);
    }
}