using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class CookingManager : MonoBehaviour
{
    public static CookingManager Instance = null;

    [SerializeField] private GameObject dummyCookRes;

    public GameObject cookPrefab => dummyCookRes;

    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public NetworkObject SpawnCookItem()
    {
        var instance = Instantiate(dummyCookRes);
        var netObject = instance.GetComponent<NetworkObject>();
        netObject.Spawn();

        return netObject;
    }
}
