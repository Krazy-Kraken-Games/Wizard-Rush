using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class SpawnObjects : MonoBehaviour
{
    [SerializeField] private NetworkManager networkManager;

    [SerializeField] private List<GameObject> objectsToSpawn;

    private void Awake()
    {
        networkManager = GetComponent<NetworkManager>();
    }

    private void Start()
    {
        networkManager.OnServerStarted += OnServerStarted;
    }

    private void OnDestroy()
    {
        networkManager.OnServerStarted -= OnServerStarted;
    }

    private void OnServerStarted()
    {
        Debug.Log("Server has finally started");

        foreach(var obj in objectsToSpawn)
        {
            var instance = Instantiate(obj);
            var netObject = instance.GetComponent<NetworkObject>();
            netObject.Spawn();
        }
    }

    private void OnConnectedToServer()
    {
        Debug.Log("Successfully connected to a server");
    }
}
