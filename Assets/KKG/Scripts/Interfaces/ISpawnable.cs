using Unity.Netcode;
using UnityEngine;

public interface ISpawnable
{
    void SpawnEntity();
    NetworkObject SpawnEntityWithPrefab(GameObject prefab);
}
