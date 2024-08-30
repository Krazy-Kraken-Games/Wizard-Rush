using KrazyKrakenGames.Interactables;
using KrazyKrakenGames.LearningNetcode;
using System.Collections;
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

    //Dummy will be removed when actual ingredient data structure is created
    [SerializeField] private string IngredientNameTest;

    [SerializeField] private StationCanvas stationCanvas;

    //Cook handling
    [SerializeField] private NetworkVariable<StationState> CookState
        = new NetworkVariable<StationState>(StationState.READY,
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
    }

    public override void OnNetworkDespawn()
    {
        addedIngredient.OnValueChanged -= OnAddedIngredientChange;

        currentIngredients.OnListChanged -= OnIngredientListChanged;

        CookState.OnValueChanged -= OnCookValueChangedHandler;

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
        IngredientNameTest = curr.ingredientName.ToString();

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
        else if(newState == StationState.READY)
        {
            Debug.Log("Cooked item ready to be recieved!");
        }
    }

    private void StartCooking()
    {
        //Running on server

        //Determine which item will be cooked
        Debug.Log("Cooking in progress....");
        //Wait for cook time to be completed => for testing 3seconds

        Invoke("CompleteCooking", 3.15f);
    }

    private void CompleteCooking()
    {
        //After 3 seconds, give the cooked item as ready
        CookState.Value = StationState.READY;
    }

    #endregion
}
