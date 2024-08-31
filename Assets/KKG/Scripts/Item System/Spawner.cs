using KrazyKrakenGames.Interactables;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Events;

public class Spawner : NetworkBehaviour,ISpawnable
{
    //Prefab to spawn
    [SerializeField] private NetworkObject instancePrefab;

    //Reference to a spawned object, might have to make it a variable?
    [SerializeField] private NetworkObject spawnedInstance;

    [SerializeField] private float spawnCounter = 0;
    [SerializeField] private float spawnTimer = 5f;

    [Tooltip("Set to false for spawners which are not timer based")]
    [SerializeField] private bool shouldUseTimer = true;

    //Fired when the object is picked for the first time
    public UnityEvent OnObjectPickedEvent;

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

    public NetworkObject SpawnEntityWithPrefab(GameObject prefab)
    {
        if (IsServer)
        {
            var obj = Instantiate(prefab, prefab.transform.position, Quaternion.identity);
            var netObject = obj.GetComponent<NetworkObject>();
            netObject.Spawn(true);

            spawnedInstance = netObject;

            RegisterEvent();

            return netObject;
        }

        return null;
    }

    private void RegisterEvent()
    {
        var baseIntObj = spawnedInstance.
            GetComponent<BaseInteractableObject>();
        baseIntObj.State.OnValueChanged += OnSpawnedInstanceStateChanged;

        baseIntObj.SetSpawner(this);
    }

    private void UnregisterEvent()
    {
        OnObjectPickedEvent?.Invoke();

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
        if (instancePrefab == null) return;
        if(spawnedInstance == null)
        {
            SpawnEntity();
        }
    }

    public void OnObjectPicked()
    {
        var baseIntObj = spawnedInstance.
            GetComponent<BaseInteractableObject>();

        baseIntObj.SetSpawner(null);
        spawnedInstance = null;
    }

    private void Update()
    {
        if (!shouldUseTimer) return;

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
