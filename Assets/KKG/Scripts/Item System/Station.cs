using KrazyKrakenGames.Interactables;
using KrazyKrakenGames.LearningNetcode;
using Unity.Collections;
using Unity.Netcode;
using UnityEngine;

public class Station : NetworkBehaviour, ITriggerable, IStoring, ICooking
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

    [SerializeField]
    public NetworkList<IngredientData> currentIngredients;

    //Spawner reference
    [SerializeField] private Spawner spawner;

    [SerializeField] private StationCanvas stationCanvas;

    //Cook handling
    [SerializeField] private IngredientType allowedType;

    [SerializeField] private NetworkVariable<StationState> CookState
        = new NetworkVariable<StationState>(StationState.READY,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    [SerializeField]
    private NetworkVariable<IngredientData> collectItem
        = new NetworkVariable<IngredientData>(new IngredientData
            { 
                ingredientName = new FixedString128Bytes(),
                gameObjectID = 20000
            },
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    void Awake()
    {
        //NetworkList can't be initialized at declaration time like NetworkVariable. It must be initialized in Awake instead.
        //If you do initialize at declaration, you will run into memory leak errors.
        currentIngredients = new NetworkList<IngredientData>();
    }

    //Move start and destroy content to On Network Spawns when actually spawning occurs on network
    public override void OnNetworkSpawn()
    {
        addedIngredient.OnValueChanged += OnAddedIngredientChange;

        currentIngredients.OnListChanged += OnIngredientListChanged;

        CookState.OnValueChanged += OnCookValueChangedHandler;

        collectItem.OnValueChanged += OnCollectItemValueHandler;
    }

    public override void OnNetworkDespawn()
    {
        addedIngredient.OnValueChanged -= OnAddedIngredientChange;

        currentIngredients.OnListChanged -= OnIngredientListChanged;

        CookState.OnValueChanged -= OnCookValueChangedHandler;

        collectItem.OnValueChanged -= OnCollectItemValueHandler;

    }

    #region IStoring Interface Method Handling
    public bool CheckAllowed(IngredientType _type)
    {
        if (_type == allowedType) return true;

        return false;
    }

    public void AddItem(IngredientData data, ulong feederID, ulong itemID)
    {
        if (CookState.Value == StationState.READY)
        {
            

            //Lets add the items without any checks
            AddItemServerRpc(data.ingredientName, feederID, itemID);
        }
    }

    #endregion

    [ServerRpc(RequireOwnership = false)]
    private void AddItemServerRpc(FixedString128Bytes item, ulong feederID,ulong itemID)
    {
        addedIngredient.Value = new IngredientData
        {
            ingredientName = item,
            gameObjectID = itemID,   
        };

        //Adding it to list of ingredients
        currentIngredients.Add(addedIngredient.Value);

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
        if (stationCanvas != null)
        {
            stationCanvas.PopulateIngredient(curr.ingredientName.ToString());
        }
       
    }

    private void OnIngredientListChanged(NetworkListEvent<IngredientData> ingredientChange)
    {
        Debug.Log($"Current Ingredient added: {ingredientChange.Value.ingredientName.ToString()}");

        PopulateUI();
    }

    private void PopulateUI()
    {
        if (stationCanvas == null) return;

        string ingredientList = string.Empty;

        foreach (var ingredient in currentIngredients)
        {
            ingredientList += $"{ingredient.ingredientName}\n";
        }

        stationCanvas.PopulateIngredient(ingredientList);
    }

    #region Cookable Interface Handling
    public void CheckIngredients()
    {
        //Logic will be added later
    }

    public void Cook()
    {
        //Check the ingredients
        CheckIngredients();

        CookOnServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    private void CookOnServerRpc()
    {
        if (currentIngredients.Count > 0)
        {
            Debug.Log("Cook with the ingredients we have!");
            CookState.Value = StationState.COOK;

            //Give status the cooked item is ready
            StartCooking();
        }
    }

    private void OnCookValueChangedHandler(StationState prev, StationState newState)
    {
        if (newState == StationState.COOK)
        {
            Debug.Log("On clients! Starting cook state");

            if(stationCanvas != null)
            {
                stationCanvas.StationCooking(3f);
            }
        }
        else if(newState == StationState.COLLECT)
        {
            Debug.Log("Cooked item ready to be recieved!");
        }
        else if(newState == StationState.READY)
        {
            //Check if prev state was collect
            if(prev == StationState.COLLECT)
            {
                if(stationCanvas != null)
                {
                    stationCanvas.Reset();
                }
            }
        }
    }

    private void StartCooking()
    {
        //Running on server

        //Determine which item will be cooked
        currentIngredients.Clear();
        addedIngredient.Value =
            new IngredientData
            {
                ingredientName = new FixedString128Bytes(),
                gameObjectID = 10000
            };

        Debug.Log("Cooking in progress....");
        //Wait for cook time to be completed => for testing 3seconds

        Invoke("CompleteCooking", 3.15f);
    }

    [ServerRpc(RequireOwnership = false)]
    private void CompleteCookingServerRpc()
    {
        //After 3 seconds, give the cooked item as ready
        CookState.Value = StationState.COLLECT;

        var objPrefab = CookingManager.Instance.cookPrefab;
        var ingredient = objPrefab.GetComponent<Ingredient>();


        var spawnedInstance = spawner.SpawnEntityWithPrefab(objPrefab);

        collectItem.Value =
            new IngredientData
            {
                ingredientName = ingredient.ingredientData.ingredientName,
                gameObjectID = spawnedInstance.NetworkObjectId
            };
    }

    private void CompleteCooking()
    {
        CompleteCookingServerRpc();
    }

    #endregion

    #region Collect Item Handling

    public void OnObjectCollected()
    {
        Debug.Log($"Object {collectItem.Value.ingredientName} was collected from {gameObject.name}");
        ObjectCollectedServerRpc();
    }

    [ServerRpc(RequireOwnership = false)]
    public void ObjectCollectedServerRpc()
    {
        //Object was collected, set collect Item to null/default and 
        //set state to ready for cooking
        collectItem.Value = new IngredientData
        {
            ingredientName = new FixedString128Bytes(),
            gameObjectID = 20000
        };

        CookState.Value = StationState.READY;
    }

    private void OnCollectItemValueHandler(IngredientData prev, IngredientData curr)
    {
        
    }


    #endregion

}
