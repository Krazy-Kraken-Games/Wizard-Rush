using KrazyKrakenGames.LearningNetcode;
using Unity.Netcode;
using UnityEngine;
using static UnityEditor.Progress;

public class Cannon : NetworkBehaviour, ITriggerable, IStoring,IControlBody
{
    #region Variables
    public GameObject GameObject => gameObject;

    [SerializeField] private IngredientType allowedType;

    //Temporary variable, number of ammo the gun currently has loaded in
    //Will be switched to using a network list once bullet prefab is done
    [SerializeField] private NetworkVariable<int> ammo 
        = new NetworkVariable<int>(0,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    [SerializeField] private bool hasAmmo;

    #endregion

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        ammo.OnValueChanged += OnAmmoValueChanged;
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();

        ammo.OnValueChanged -= OnAmmoValueChanged;
    }

    #region IStoring Interface Handlers

    #region Ammo Storage Handling
    public void AddItem(IngredientData data, ulong feederID, ulong itemID)
    {
        AddAmmoServerRpc(feederID);
    }

    //Any player can add the ammo to the gun
    [ServerRpc(RequireOwnership = false)]
    private void AddAmmoServerRpc(ulong feederID)
    {
        //Make holder drop the item
        var feederObj =
            NetGameManager.instance.GetNetworkObjectById(feederID);

        feederObj.GetComponent<PickUpHolder>().DropPickedObjectServerRpc();


        ammo.Value++;
    }


    private void OnAmmoValueChanged(int prevAmmo,int currAmmo)
    {
        hasAmmo = currAmmo > 0;
    }

    #endregion

    public bool CheckAllowed(IngredientType _type)
    {
        return allowedType == _type;
    }
    #endregion

    #region Cannon Body Control
    public void TakeControl(ulong _objectID)
    {
        if (!hasAmmo) return;

        var playerID =
            NetGameManager.instance.GetNetworkObjectById(_objectID);

        playerID.GetComponent<PlayerStateHandler>().SetState(PlayerState.CANNON);

        Debug.Log("Controlling the cannon now");
    }
    #endregion
}
