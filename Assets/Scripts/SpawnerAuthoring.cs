using Unity.Entities;
using UnityEngine;
using UnityEngine.Serialization;

public struct Spawner : IComponentData
{
    public Entity SpawnableEntity;
}

[DisallowMultipleComponent]
public class SpawnerAuthoring : MonoBehaviour
{
    public GameObject Entity;

    class Baker : Baker<SpawnerAuthoring>
    {
        public override void Bake(SpawnerAuthoring authoring)
        {
            Spawner component = default(Spawner);
            component.SpawnableEntity = GetEntity(authoring.Entity, TransformUsageFlags.Dynamic);
            AddComponent(component);
        }
    }
}