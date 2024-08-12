using KrazyKrakenGames.Interactables;
using KrazyKrakenGames.UI;
using System;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

[Serializable]
public enum PickUpHolderState
{
    HIDDEN = 0,
    VISIBLE = 1
}

public class PickUpHolder : NetworkBehaviour
{
    [SerializeField]
    private NetworkVariable<PickUpHolderState>
        NetworkState = new NetworkVariable<PickUpHolderState>(PickUpHolderState.HIDDEN,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    //Will be converted to using held object information
    [SerializeField]
    private NetworkVariable<FixedString128Bytes> heldObject
        = new NetworkVariable<FixedString128Bytes>(string.Empty,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    //Has object in hand
    [SerializeField]
    private NetworkVariable<bool> hasObject
        = new NetworkVariable<bool>(false,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    [SerializeField] private string heldObjectName;

    [SerializeField] private Transform holderTransform;
    [SerializeField] private GameObject pickUpMesh;
    [SerializeField] private bool initialized = false;


    [SerializeField] private GameObject pickUpMeshPrefab;
    [SerializeField] private NetworkObject n_spawnedPickMesh;

    [SerializeField] private PlayerInputHandling playerInput;

    [SerializeField] private PlayerCanvas displayCanvas;


    private void Awake()
    {
        playerInput = GetComponent<PlayerInputHandling>();
        NetworkState.OnValueChanged += OnStateValueChangedListener;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerInput.OnInteractButtonFired += OnPlayerInteractionHandler;
        playerInput.OnActionEventHandler += OnPlayerActionKeyHandler;

        heldObject.OnValueChanged += OnHeldObjectChangedHandler;
        
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkState.OnValueChanged -= OnStateValueChangedListener;

        playerInput.OnInteractButtonFired -= OnPlayerInteractionHandler;
        playerInput.OnActionEventHandler -= OnPlayerActionKeyHandler;

        heldObject.OnValueChanged -= OnHeldObjectChangedHandler;

    }
    private void Initialization()
    {
        RegisterPickUpServerRpc();
    }

    private void Update()
    {
        if(!IsOwner) return;
    }

    private void OnStateValueChangedListener(PickUpHolderState prev, PickUpHolderState newState)
    {
        if (newState == PickUpHolderState.VISIBLE)
        {
            pickUpMesh.SetActive(true);
        }
        else
        {
            pickUpMesh.SetActive(false);
        }
    }

    public void OnPickUpInitiated(BaseInteractableObject interactableObject)
    {
        if (interactableObject != null)
        {
            //Check if already carrying an item
            if (hasObject.Value)
            {
                Vector3 dropLocation = transform.position + (transform.forward * 2f);
                //Drop
                interactableObject.Drop(NetworkObjectId, dropLocation);
            }
            else
            {
                interactableObject.Interact(NetworkObjectId);
            }
        }
       
    }


    [ServerRpc(RequireOwnership = false)]
    public void RegisterPickUpServerRpc(ServerRpcParams rpcParams = default)
    {
        pickUpMesh = Instantiate(pickUpMeshPrefab);
        n_spawnedPickMesh = pickUpMesh.GetComponent<NetworkObject>();
        n_spawnedPickMesh.SpawnWithOwnership(OwnerClientId);
        n_spawnedPickMesh.TrySetParent(transform,false);

        ulong id = n_spawnedPickMesh.NetworkObjectId;
        FindPickUpClientRpc(id, OwnerClientId);

    }

    [ClientRpc]
    public void FindPickUpClientRpc(ulong objectId, ulong ownerClientId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var obj))
        {
            pickUpMesh = obj.gameObject;

            if (ownerClientId == OwnerClientId)
            {
                initialized = true;
            }
        }
    }

    private void OnPlayerActionKeyHandler(GameObject nearByStation)
    {
        if (nearByStation == null) return;

        Debug.Log("Action button is pressed");

        if (hasObject.Value)
        {
            //Only feed ingredient if player is carrying an object
            Debug.Log("Submitting held object into the station");
            var storing = nearByStation.GetComponent<IStoring>();
            storing.AddItem(heldObject.Value);
        }
    }

    public void OnPlayerInteractionHandler(BaseInteractableObject interactionObject)
    {
        if (!initialized)
        {
            Initialization();
        }
        else
        {
            //Has Pick up objects initialized, work on it after drop
        }
    }


    public void SetPickedObject(string pickedItem)
    {
        Debug.Log($"{pickedItem} has been picked");

        if (!initialized)
        {
            Initialization();
        }

        SetPickedObjectServerRpc(pickedItem);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPickedObjectServerRpc(string pickedItem)
    {
        heldObject.Value = pickedItem;
        hasObject.Value = true;

        NetworkState.Value = PickUpHolderState.VISIBLE;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DropPickedObjectServerRpc()
    {
        heldObject.Value = new FixedString128Bytes();
        hasObject.Value = false;

        NetworkState.Value = PickUpHolderState.HIDDEN;
        DropResetClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void DropResetClientRpc()
    {
        gameObject.GetComponent<PlayerInputHandling>().SetInteractionObject(null);
    }


    private void OnHeldObjectChangedHandler(FixedString128Bytes prev, FixedString128Bytes curr)
    {
        displayCanvas.PopulateObjectPick(curr.ToString());
    }


}
