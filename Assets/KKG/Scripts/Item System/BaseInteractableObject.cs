using KrazyKrakenGames.Interfaces.Objects;
using KrazyKrakenGames.LearningNetcode;
using System;
using Unity.Netcode;
using UnityEngine;

namespace KrazyKrakenGames.Interactables
{
    [Serializable]
    public enum ObjectState
    {
        DROP = 0,
        PICK = 1,
        FEED = 2 //Item feeded will never come back
    }

    /// <summary>
    /// A base interactable object class to hold interact with
    /// </summary>
    public class BaseInteractableObject : NetworkBehaviour, 
        IInteractable,IPickable, ITriggerable
    {
        public GameObject GameObject => gameObject;

        public NetworkVariable<ObjectState> State =
            new NetworkVariable<ObjectState>(ObjectState.DROP
                ,NetworkVariableReadPermission.Everyone,
                NetworkVariableWritePermission.Server);

        //Turn this to use data from scriptable object
        public IngredientDataSO ingredientData;


        //Who spawned this object
        [SerializeField] protected ISpawnable spawnedParent;


        public override void OnNetworkSpawn()
        {
            State.OnValueChanged += OnStateValueChanged;
        }

        public override void OnNetworkDespawn()
        {
            State.OnValueChanged -= OnStateValueChanged;
        }

        private void OnStateValueChanged(ObjectState prev, ObjectState curr)
        {
            if (curr == ObjectState.DROP)
            {
                gameObject.SetActive(true);
            }
            else if (curr == ObjectState.PICK)
            {
                gameObject.SetActive(false);
            }
            else if (curr == ObjectState.FEED)
            {
                NetworkObject.Despawn(gameObject);
                Destroy(gameObject);
            }
        }

        public virtual void Interact(ulong initiatorID)
        {
            Debug.Log($"{ingredientData} is interacted with by {initiatorID}");

            if(State.Value == ObjectState.DROP)
            {
                //Then object can be picked
                Pick(initiatorID);
            }
        }

        public void Pick(ulong initiatorID)
        {
            PickServerRpc(initiatorID);
        }

        [ServerRpc(RequireOwnership = false)]
        public void PickServerRpc(ulong initiatorID)
        {
            PickRpcHelper(initiatorID);
        }

        protected virtual void PickRpcHelper(ulong initiatorID)
        {
            var initiator = NetGameManager.instance.GetNetworkObjectById(initiatorID);

            var picker = initiator.gameObject.GetComponent<PickUpHolder>();
            picker.SetPickedObject(ingredientData.ingredientName, NetworkObjectId);

            State.Value = ObjectState.PICK;
        }

        public void Drop(ulong initiatorID,Vector3 dropLocation)
        {
            DropServerRpc(initiatorID, dropLocation);
        }

        [ServerRpc(RequireOwnership = false)]
        public void DropServerRpc(ulong initiatorID,Vector3 dropLocation)
        {
            transform.position = dropLocation;
            var initiator = NetGameManager.instance.GetNetworkObjectById(initiatorID);

            var picker = initiator.gameObject.GetComponent<PickUpHolder>();
            picker.DropPickedObjectServerRpc();

            State.Value = ObjectState.DROP;
           
        }

        public void Feed(ulong initiatorID)
        {
            FeedServerRpc();
        }

        [ServerRpc(RequireOwnership = false)]
        private void FeedServerRpc()
        {
            State.Value = ObjectState.FEED;
        }

        public void SetSpawner(ISpawnable _spawner)
        {
            Debug.Log($"{gameObject.name} spawned parent is {_spawner}");
            spawnedParent = _spawner;
        }
    }
}
