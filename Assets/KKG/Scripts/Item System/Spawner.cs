using KrazyKrakenGames.Interactables;
using Unity.Netcode;
using UnityEngine;

public class Spawner : NetworkBehaviour,ISpawnable
{
    //Prefab to spawn
    [SerializeField] private NetworkObject instancePrefab;

    //Reference to a spawned object, might have to make it a variable?
    [SerializeField] private NetworkObject spawnedInstance;

    [SerializeField] private float spawnCounter = 0;
    [SerializeField] private float spawnTimer = 5f; 

    public void SpawnEntity()
    {
        if(IsServer)
        {
            var obj = Instantiate(instancePrefab,transform.position,Quaternion.identity);
            obj.Spawn(true);

            spawnedInstance = obj;

            RegisterEvent();
        }
    }

    private void RegisterEvent()
    {
        spawnedInstance.
            GetComponent<BaseInteractableObject>().
            State.OnValueChanged += OnSpawnedInstanceStateChanged;
    }

    private void UnregisterEvent()
    {
        spawnedInstance.
           GetComponent<BaseInteractableObject>().
           State.OnValueChanged -= OnSpawnedInstanceStateChanged;
    }

    private void OnSpawnedInstanceStateChanged(ObjectState prev, ObjectState curr)
    {
        if (curr == ObjectState.PICK)
        {
            //Object was picked, remove spawned Instance

            UnregisterEvent();
            OnObjectPicked();
        }
    }

    public override void OnNetworkSpawn()
    {
        if(spawnedInstance == null)
        {
            SpawnEntity();
        }
    }

    public void OnObjectPicked()
    {
        spawnedInstance = null;
    }

    private void Update()
    {
        if (IsServer)
        {
            if (spawnedInstance == null)
            {
                spawnCounter += Time.deltaTime;

                if (spawnCounter > spawnTimer)
                {
                    SpawnEntity();
                    spawnCounter = 0;
                }
            }
        }
    }


}
