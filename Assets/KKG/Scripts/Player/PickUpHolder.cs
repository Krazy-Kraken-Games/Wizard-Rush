using KrazyKrakenGames.Interactables;
using KrazyKrakenGames.LearningNetcode;
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
    private static readonly string empty = string.Empty;

    //Will be converted to using held object information
    [SerializeField]
    private NetworkVariable<IngredientData> heldObject
        = new NetworkVariable<IngredientData>(new IngredientData
        {
            ingredientName = "IngredientTest",
            gameObjectID = 1000
        },
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

    #region Player State Handling

    [Space(10)]
    [Header("Player State Handler References")]
    [SerializeField] private PlayerStateHandler playerStateHandler;
    [SerializeField] private bool inputAllowed;

    #endregion


    private void Awake()
    {
        playerInput = GetComponent<PlayerInputHandling>();
        playerStateHandler = GetComponent<PlayerStateHandler>();
        NetworkState.OnValueChanged += OnStateValueChangedListener;
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        playerInput.OnInteractButtonFired += OnPlayerInteractionHandler;
        playerInput.OnActionEventHandler += OnPlayerActionKeyHandler;

        heldObject.OnValueChanged += OnHeldObjectChangedHandler;

        playerStateHandler.State.OnValueChanged += OnPlayerStateChangedHandler;
        OnPlayerStateLogicHandler(playerStateHandler.State.Value);
    }

    public override void OnNetworkDespawn()
    {
        base.OnNetworkDespawn();
        NetworkState.OnValueChanged -= OnStateValueChangedListener;

        playerInput.OnInteractButtonFired -= OnPlayerInteractionHandler;
        playerInput.OnActionEventHandler -= OnPlayerActionKeyHandler;

        heldObject.OnValueChanged -= OnHeldObjectChangedHandler;

        playerStateHandler.State.OnValueChanged -= OnPlayerStateChangedHandler;

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
        if (hasObject.Value)
        {
            if(interactableObject == null)
            {
                //A bug that is happening, Get Interactable object on its own
                var held = NetGameManager.instance.GetNetworkObjectById(heldObject.Value.gameObjectID);
                interactableObject = held.GetComponent<BaseInteractableObject>();
            }


            //Already carrying something, drop it
            Vector3 dropLocation = transform.position + (transform.forward * 2f);
            //Drop
            interactableObject.Drop(NetworkObjectId, dropLocation);
        }
        else
        {
            if (interactableObject != null)
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

            //Check if allowed
            bool allowed = storing.CheckAllowed(heldObject.Value.ingredientType);

            if (allowed)
            {
                storing.AddItem(heldObject.Value,
                    NetworkObjectId,
                    heldObject.Value.gameObjectID);

            }
            else
            {
                //Ingredient type is not accepted
            }
        }
        else
        {
            //The player isnt carrying anything, it wants to trigger cook on the station
            if (nearByStation.TryGetComponent<ICooking>(out var cookingStation))
            {
                cookingStation.Cook();
            }

            //If its not a cooking station, maybe its a cannon to be interacted with, control
            else if(nearByStation.TryGetComponent<IControlBody>(out var controlBody))
            {
                controlBody.TakeControl(NetworkObjectId);
            }
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


    public void SetPickedObject(IngredientDataSO pickedItem, ulong itemID)
    {
        Debug.Log($"{pickedItem.ingredientName} has been picked by player");

        if (!initialized)
        {
            Initialization();
        }

        SetPickedObjectServerRpc(pickedItem.ingredientName, itemID,pickedItem.ingredientType);
    }

    [ServerRpc(RequireOwnership = false)]
    public void SetPickedObjectServerRpc
        (string ingredientName,ulong itemId, IngredientType ingredientType)
    {
        heldObject.Value = new IngredientData 
        {
            ingredientName = ingredientName, 
            gameObjectID = itemId,
            ingredientType = ingredientType
        };

        hasObject.Value = true;

        NetworkState.Value = PickUpHolderState.VISIBLE;
    }

    [ServerRpc(RequireOwnership = false)]
    public void DropPickedObjectServerRpc()
    {
        heldObject.Value = new IngredientData { ingredientName = new FixedString128Bytes(), gameObjectID = 1000 };
        hasObject.Value = false;

        NetworkState.Value = PickUpHolderState.HIDDEN;
        DropResetClientRpc();
    }

    [ClientRpc(RequireOwnership = false)]
    private void DropResetClientRpc()
    {
        gameObject.GetComponent<PlayerInputHandling>().SetInteractionObject(null);
    }


    private void OnHeldObjectChangedHandler(IngredientData prev, IngredientData curr)
    {
        displayCanvas.PopulateObjectPick(curr.ingredientName.ToString());
    }


    #region Player State Change Handler

    private void OnPlayerStateChangedHandler(PlayerState prev, PlayerState newState)
    {
        OnPlayerStateLogicHandler(newState);
    }

    private void OnPlayerStateLogicHandler(PlayerState _state)
    {
        inputAllowed = _state != PlayerState.CANNON;
    }

    #endregion

}
