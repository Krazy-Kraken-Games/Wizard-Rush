using Unity.Netcode;
using UnityEngine;

public class Cannon : NetworkBehaviour, ITriggerable, IStoring
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

    #endregion

    #region IStoring Interface Handlers

    #region Ammo Storage Handling
    public void AddItem(IngredientData data, ulong feederID, ulong itemID)
    {
        AddAmmoServerRpc();
    }

    //Any player can add the ammo to the gun
    [ServerRpc(RequireOwnership = false)]
    private void AddAmmoServerRpc()
    {
        ammo.Value++;
    }

    #endregion

    public bool CheckAllowed(IngredientType _type)
    {
        return allowedType == _type;
    }

    #endregion
}
