using KrazyKrakenGames.Interactables;
using KrazyKrakenGames.LearningNetcode;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Station : NetworkBehaviour, ITriggerable, IStoring
{
    public GameObject GameObject => gameObject;

    [SerializeField]
    private NetworkVariable<IngredientData> addedIngredient
      = new NetworkVariable<IngredientData>(new IngredientData
      {
          ingredientName = new FixedString128Bytes(),
          gameObjectID = 10000
      },
          NetworkVariableReadPermission.Everyone,
          NetworkVariableWritePermission.Server);

    //Dummy will be removed when actual ingredient data structure is created
    [SerializeField] private string IngredientNameTest;

    [SerializeField] private StationCanvas stationCanvas;

    //Move start and destroy content to On Network Spawns when actually spawning occurs on network
    public override void OnNetworkSpawn()
    {
        addedIngredient.OnValueChanged += OnAddedIngredientChange;
    }

    public override void OnNetworkDespawn()
    {
        addedIngredient.OnValueChanged -= OnAddedIngredientChange;
    }

    public void AddItem(FixedString128Bytes item, ulong feederID, ulong itemID)
    {
        //Lets add the items without any checks
        AddItemServerRpc(item,feederID,itemID);
    }

    [ServerRpc(RequireOwnership = false)]
    private void AddItemServerRpc(FixedString128Bytes item, ulong feederID,ulong itemID)
    {
        addedIngredient.Value = new IngredientData
        {
            ingredientName = item,
            gameObjectID = itemID,   
        };

        //Make holder drop the item
        var feederObj = 
            NetGameManager.instance.GetNetworkObjectById(feederID);

        feederObj.GetComponent<PickUpHolder>().DropPickedObjectServerRpc();

        //Changed network ID state to notify that it was fed

        var ingredientObj = 
            NetGameManager.instance.GetNetworkObjectById(itemID);

        ingredientObj.GetComponent<BaseInteractableObject>().Feed(feederID);
    }

    private void OnAddedIngredientChange(IngredientData old, IngredientData curr)
    {
        IngredientNameTest = curr.ingredientName.ToString();

        if (stationCanvas != null)
        {
            stationCanvas.PopulateIngredient(curr.ingredientName.ToString());
        }
    }
}
