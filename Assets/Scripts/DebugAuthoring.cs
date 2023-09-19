
using Unity.Entities;
using Unity.Mathematics;
using UnityEngine;



public class DebugAuthoring : MonoBehaviour
{
}

public class DebugAuthoringBaker : Baker<DebugAuthoring>
{
    public override void Bake(DebugAuthoring authoring)
    {
        var playerEntity = GetEntity(TransformUsageFlags.Dynamic);

        AddComponent<DebugTag>(playerEntity);
        AddComponent<DebugInput>(playerEntity);
        
    }
}